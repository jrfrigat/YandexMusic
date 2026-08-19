using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using YandexMusic.Exceptions;
using YandexMusic.Http;
using YandexMusic.Serialization;

namespace YandexMusic.Ynison;

/// <summary>
/// The Ynison client: a long-lived websocket subscription to the account's playback state and a
/// channel for remote-control commands. Ynison is what synchronizes the web player, the phone apps
/// and smart speakers, so this client can observe what is playing anywhere and control it.
///
/// The usage pattern mirrors the reference implementations: start <see cref="RunAsync"/> in a
/// background task (it performs the redirect handshake, connects the state socket and keeps
/// reconnecting with a capped exponential backoff), await the first frame with
/// <see cref="WaitForStateAsync"/>, then either react to <see cref="StateReceived"/> or send
/// commands via <see cref="SendAsync"/> and the convenience methods. Dispose the client to stop.
/// </summary>
public sealed class YnisonClient : IAsyncDisposable
{
    private const string RedirectService = "redirector.YnisonRedirectService/GetRedirectToYnison";
    private const string StateService = "ynison_state.YnisonStateService/PutYnisonState";
    private static readonly TimeSpan DefaultKeepAlive = TimeSpan.FromSeconds(20);

    private readonly string _token;
    private readonly YnisonClientOptions _options;
    private readonly IYnisonSocketFactory _socketFactory;

    private readonly TaskCompletionSource _firstState = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IYnisonSocket? _redirectSocket;
    private IYnisonSocket? _stateSocket;
    private PutYnisonStateResponse? _latestState;
    private int _stopping;

    /// <summary>Creates a client with a generated device id and default options.</summary>
    /// <param name="token">The OAuth token of the account.</param>
    public YnisonClient(string token)
        : this(token, null, null)
    {
    }

    /// <summary>Creates a client with an explicit device id and options.</summary>
    /// <param name="token">The OAuth token of the account.</param>
    /// <param name="deviceId">
    /// This client's identifier in the Ynison session; fix it across runs, or every run registers
    /// as a new device. Generated when <see langword="null"/>.
    /// </param>
    /// <param name="options">The client options, or <see langword="null"/> for defaults.</param>
    /// <exception cref="ArgumentException"><paramref name="token"/> is null or whitespace.</exception>
    public YnisonClient(string token, string? deviceId, YnisonClientOptions? options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        _token = token;
        DeviceId = string.IsNullOrWhiteSpace(deviceId) ? YnisonRequests.GenerateDeviceId() : deviceId;
        _options = options ?? new YnisonClientOptions();
        _socketFactory = new ClientWebSocketYnisonSocketFactory();
    }

    internal YnisonClient(string token, string? deviceId, YnisonClientOptions? options, IYnisonSocketFactory socketFactory)
        : this(token, deviceId, options)
    {
        _socketFactory = socketFactory;
    }

    /// <summary>Raised for every state frame received from Ynison, after <see cref="LatestState"/> was updated.</summary>
    public event EventHandler<PutYnisonStateResponse>? StateReceived;

    /// <summary>
    /// Raised when a <see cref="StateReceived"/> handler throws. Handler failures are isolated so
    /// they cannot break the receive loop; when no handler is attached the failure is ignored.
    /// </summary>
    public event EventHandler<Exception>? ListenerError;

    /// <summary>This client's identifier in the Ynison session.</summary>
    public string DeviceId { get; }

    /// <summary>The most recent state frame, or <see langword="null"/> before the first one arrives.</summary>
    public PutYnisonStateResponse? LatestState => Volatile.Read(ref _latestState);

    /// <summary>Waits for the first state frame, so commands can be built from a known state.</summary>
    /// <param name="timeout">How long to wait for the frame.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The first state frame.</returns>
    /// <exception cref="YandexMusicYnisonException">No frame arrived within <paramref name="timeout"/>.</exception>
    public async Task<PutYnisonStateResponse> WaitForStateAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var latest = LatestState;
        if (latest is not null)
        {
            return latest;
        }

        try
        {
            await _firstState.Task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException ex)
        {
            throw new YandexMusicYnisonException($"No Ynison state arrived within {timeout}.", ex);
        }

        return LatestState ?? throw new YandexMusicYnisonException("The Ynison connection closed before the first state frame.");
    }

    /// <summary>
    /// Runs the connection until the client is disposed, the token is cancelled, or the server
    /// closes the state socket gracefully. Transport failures reconnect with a capped exponential
    /// backoff; protocol failures (malformed frames) throw
    /// <see cref="YandexMusicYnisonException"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to stop the client.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var reconnectNumber = 0;
        while (Volatile.Read(ref _stopping) == 0 && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunConnectionAsync(cancellationToken).ConfigureAwait(false);

                // A graceful close (by the server or by DisposeAsync) ends the run, like the
                // reference implementations.
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || Volatile.Read(ref _stopping) == 1)
            {
                return;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                if (Volatile.Read(ref _stopping) == 1)
                {
                    return;
                }

                reconnectNumber++;
                var backoff = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, reconnectNumber), 64) + (Random.Shared.NextDouble() - 0.5));
                try
                {
                    await Task.Delay(backoff, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    /// <summary>Sends an arbitrary state update, normally built by <see cref="YnisonRequests"/>.</summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the send.</param>
    /// <exception cref="YandexMusicYnisonException">The state socket is not connected.</exception>
    public async Task SendAsync(PutYnisonStateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var socket = Volatile.Read(ref _stateSocket)
            ?? throw new YandexMusicYnisonException("The Ynison state socket is not connected; start RunAsync first.");
        var json = JsonSerializer.Serialize(request, YnisonJson.TypeInfo<PutYnisonStateRequest>());
        await socket.SendAsync(json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Pauses or resumes playback on the active device.</summary>
    /// <param name="paused"><see langword="true"/> to pause, <see langword="false"/> to resume.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task SetPausedAsync(bool paused, CancellationToken cancellationToken = default)
    {
        var state = RequireState();
        return SendAsync(
            YnisonRequests.CreateSetPausedRequest(DeviceId, RequireStatus(state), paused),
            cancellationToken);
    }

    /// <summary>Switches the active device to the next track of its queue.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task NextTrackAsync(CancellationToken cancellationToken = default)
        => SendAsync(YnisonRequests.CreateNextTrackRequest(DeviceId, RequirePlayerState()), cancellationToken);

    /// <summary>Switches the active device to the previous track of its queue.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task PreviousTrackAsync(CancellationToken cancellationToken = default)
        => SendAsync(YnisonRequests.CreatePreviousTrackRequest(DeviceId, RequirePlayerState()), cancellationToken);

    /// <summary>Changes the volume of a device of the session.</summary>
    /// <param name="targetDeviceId">The device whose volume changes.</param>
    /// <param name="volume">The new volume in [0.0; 1.0].</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task SetVolumeAsync(string targetDeviceId, double volume, CancellationToken cancellationToken = default)
        => SendAsync(YnisonRequests.CreateSetVolumeRequest(DeviceId, targetDeviceId, volume), cancellationToken);

    /// <summary>Makes another device the active one; it takes over the session's playback.</summary>
    /// <param name="targetDeviceId">The device that should play the sound.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task SetActiveDeviceAsync(string targetDeviceId, CancellationToken cancellationToken = default)
        => SendAsync(YnisonRequests.CreateSetActiveDeviceRequest(targetDeviceId), cancellationToken);

    /// <summary>
    /// Starts playback of the current track on a device: makes it the active one and resumes. The
    /// new device continues from the session's current progress.
    /// </summary>
    /// <param name="targetDeviceId">The device to start playback on.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public async Task PlayOnDeviceAsync(string targetDeviceId, CancellationToken cancellationToken = default)
    {
        await SetActiveDeviceAsync(targetDeviceId, cancellationToken).ConfigureAwait(false);
        await SetPausedAsync(paused: false, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _stopping, 1) == 1)
        {
            return;
        }

        _firstState.TrySetCanceled();

        var sockets = new[] { Volatile.Read(ref _redirectSocket), Volatile.Read(ref _stateSocket) };
        foreach (var socket in sockets)
        {
            if (socket is not null)
            {
                await socket.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task RunConnectionAsync(CancellationToken cancellationToken)
    {
        // Phase 1: the redirector hands out the state host plus a one-time ticket.
        var redirect = _socketFactory.Create();
        Volatile.Write(ref _redirectSocket, redirect);
        try
        {
            await redirect.ConnectAsync(
                BuildUri(RedirectService),
                BuildSubprotocols(ticket: null, sessionId: null),
                BuildHeaders(),
                DefaultKeepAlive,
                cancellationToken).ConfigureAwait(false);

            var redirectJson = await redirect.ReceiveTextAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new YandexMusicYnisonException("The Ynison redirector closed the connection before answering.");

            RedirectResponse? redirection;
            try
            {
                redirection = JsonSerializer.Deserialize(redirectJson, YnisonJson.TypeInfo<RedirectResponse>());
            }
            catch (JsonException ex)
            {
                throw new YandexMusicYnisonException("The Ynison redirector returned a malformed frame.", ex);
            }

            if (redirection is null
                || string.IsNullOrEmpty(redirection.Host)
                || string.IsNullOrEmpty(redirection.RedirectTicket)
                || redirection.SessionId == 0)
            {
                throw new YandexMusicYnisonException($"The Ynison redirector returned an incomplete answer: {redirectJson}");
            }

            // Phase 2: the state socket on the redirected host, authorized by the ticket.
            var keepAlive = _options.KeepAliveInterval
                ?? (redirection.KeepAliveParams?.KeepAliveTimeSeconds > 0
                    ? TimeSpan.FromSeconds(redirection.KeepAliveParams.KeepAliveTimeSeconds)
                    : DefaultKeepAlive);

            var state = _socketFactory.Create();
            Volatile.Write(ref _stateSocket, state);
            try
            {
                await state.ConnectAsync(
                    BuildUri(StateService, redirection.Host),
                    BuildSubprotocols(redirection.RedirectTicket, redirection.SessionId),
                    BuildHeaders(),
                    keepAlive,
                    cancellationToken).ConfigureAwait(false);

                // The redirector socket has served its purpose; its close handshake runs while the
                // state socket comes up.
                await DisposeSocketAsync(redirect).ConfigureAwait(false);
                Volatile.Write(ref _redirectSocket, null);

                await SendAsync(YnisonRequests.CreateUpdateFullStateRequest(DeviceId, _options.AppName), cancellationToken)
                    .ConfigureAwait(false);

                while (true)
                {
                    var frame = await state.ReceiveTextAsync(cancellationToken).ConfigureAwait(false);
                    if (frame is null)
                    {
                        // A graceful server close ends the run.
                        return;
                    }

                    PutYnisonStateResponse? response;
                    try
                    {
                        response = JsonSerializer.Deserialize(frame, YnisonJson.TypeInfo<PutYnisonStateResponse>());
                    }
                    catch (JsonException ex)
                    {
                        throw new YandexMusicYnisonException("Ynison sent a malformed state frame.", ex);
                    }

                    if (response is null)
                    {
                        continue;
                    }

                    Volatile.Write(ref _latestState, response);
                    _firstState.TrySetResult();
                    RaiseStateReceived(response);
                }
            }
            finally
            {
                await DisposeSocketAsync(state).ConfigureAwait(false);
                Volatile.Write(ref _stateSocket, null);
            }
        }
        finally
        {
            await DisposeSocketAsync(redirect).ConfigureAwait(false);
            Volatile.Write(ref _redirectSocket, null);
        }
    }

    private void RaiseStateReceived(PutYnisonStateResponse response)
    {
        var handler = StateReceived;
        if (handler is null)
        {
            return;
        }

        try
        {
            handler(this, response);
        }
        catch (Exception ex)
        {
            ListenerError?.Invoke(this, ex);
        }
    }

    private PutYnisonStateResponse RequireState()
        => LatestState ?? throw new YandexMusicYnisonException(
            "No Ynison state has been received yet; await WaitForStateAsync before sending commands.");

    private PlayerState RequirePlayerState()
        => RequireState().PlayerState
            ?? throw new YandexMusicYnisonException("The Ynison state carries no player state.");

    private static PlayingStatus RequireStatus(PutYnisonStateResponse state)
        => state.PlayerState?.Status
            ?? throw new YandexMusicYnisonException("The Ynison player state carries no playing status.");

    private Uri BuildUri(string service, string? host = null)
    {
        var builder = new StringBuilder(host is null ? _options.BaseUri : "wss://" + host);
        if (!builder[^1].Equals('/'))
        {
            builder.Append('/');
        }

        builder.Append(service);
        return new Uri(builder.ToString(), UriKind.Absolute);
    }

    private Dictionary<string, string> BuildHeaders() => new()
    {
        ["Origin"] = "https://music.yandex.ru",
        [YandexMusicHeaders.Authorization] = "OAuth " + _token,
    };

    private IReadOnlyList<string> BuildSubprotocols(string? ticket, long? sessionId)
    {
        // The third subprotocol is the device description: a JSON with an inner JSON string for the
        // device info, percent-encoded as the subprotocol token must be URL-safe.
        var deviceInfo = "{\"app_name\":\"" + JsonEscape(_options.AppName) + "\",\"type\":\"1\"}";
        var description = new StringBuilder("{\"Ynison-Device-Id\":\"")
            .Append(JsonEscape(DeviceId))
            .Append("\",\"Ynison-Device-Info\":\"")
            .Append(JsonEscape(deviceInfo))
            .Append('"');
        if (ticket is not null && sessionId is not null)
        {
            description.Append(",\"Ynison-Redirect-Ticket\":\"").Append(JsonEscape(ticket))
                .Append("\",\"Ynison-Session-Id\":\"").Append(sessionId.Value).Append('"');
        }

        description.Append('}');
        return ["Bearer", "v2", Uri.EscapeDataString(description.ToString())];
    }

    private static string JsonEscape(string value)
    {
        var escaped = new StringBuilder(value.Length + 8);
        foreach (var symbol in value)
        {
            if (symbol is '"' or '\\')
            {
                escaped.Append('\\');
            }

            escaped.Append(symbol);
        }

        return escaped.ToString();
    }

    private static async Task DisposeSocketAsync(IYnisonSocket? socket)
    {
        if (socket is not null)
        {
            await socket.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static bool IsTransient(Exception ex) => ex is WebSocketException or IOException or TimeoutException;
}
