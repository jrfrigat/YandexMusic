using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using YandexMusic.Exceptions;

namespace YandexMusic.Ynison;

/// <summary>
/// The connection side of <see cref="YnisonClient"/>: the redirect-then-state handshake, the receive
/// loop, and the reconnect policy that decides whether and when to try again.
/// </summary>
public sealed partial class YnisonClient
{
    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        var reconnectNumber = 0;
        var framesAtLastFailure = 0L;
        IReadOnlyList<TimeSpan> serverBackoff = [];
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
            catch (YandexMusicYnisonServerException ex) when (ex.IsTerminal)
            {
                // A rejected token, a displaced device or a go-away: retrying cannot succeed.
                throw;
            }
            catch (Exception ex) when (IsTransient(ex) || ex is YandexMusicYnisonServerException)
            {
                if (Volatile.Read(ref _stopping) == 1)
                {
                    return;
                }

                // A connection that did deliver frames was healthy; a later drop is a fresh problem
                // and starts the backoff over instead of inheriting the previous run's ceiling.
                var frames = Interlocked.Read(ref _framesReceived);
                if (frames != framesAtLastFailure)
                {
                    reconnectNumber = 0;
                    framesAtLastFailure = frames;
                }

                if (ex is YandexMusicYnisonServerException { Backoff.Count: > 0 } serverError)
                {
                    serverBackoff = serverError.Backoff;
                }

                reconnectNumber++;
                if (_options.MaxReconnectAttempts is { } limit && reconnectNumber > limit)
                {
                    throw new YandexMusicYnisonException($"Ynison did not reconnect after {limit} attempts.", ex);
                }

                RaiseConnectionError(ex);

                var backoff = ReconnectDelay(reconnectNumber, serverBackoff);
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

            if (YnisonServerErrors.TryParse(redirectJson, out var redirectError))
            {
                throw redirectError!;
            }

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
                await CloseSocketAsync(redirect).ConfigureAwait(false);
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

                    Raise(FrameReceived, frame);

                    // An error frame would otherwise parse as an empty state and look like
                    // "nothing is playing"; the server closes the stream after it anyway.
                    if (YnisonServerErrors.TryParse(frame, out var serverError))
                    {
                        throw serverError!;
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
                    Volatile.Write(ref _latestStateAt, Stopwatch.GetTimestamp());
                    _ = Interlocked.Increment(ref _framesReceived);
                    _ = _firstState.TrySetResult();
                    RaiseStateReceived(response);
                }
            }
            finally
            {
                await CloseSocketAsync(state).ConfigureAwait(false);
                Volatile.Write(ref _stateSocket, null);
            }
        }
        finally
        {
            await CloseSocketAsync(redirect).ConfigureAwait(false);
            Volatile.Write(ref _redirectSocket, null);
        }
    }

    // The server's own ladder wins when it sent one (with a little jitter so clients do not
    // reconnect in lockstep); otherwise a capped exponential backoff.
    private static TimeSpan ReconnectDelay(int reconnectNumber, IReadOnlyList<TimeSpan> serverBackoff)
    {
        if (serverBackoff.Count > 0)
        {
            var step = serverBackoff[Math.Min(reconnectNumber - 1, serverBackoff.Count - 1)];
            return step + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250));
        }

        return TimeSpan.FromSeconds(Math.Min(Math.Pow(2, reconnectNumber), 64) + (Random.Shared.NextDouble() - 0.5));
    }

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
        ["Authorization"] = "OAuth " + _token,
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

    /// <summary>
    /// Ends a socket the way the protocol expects: a close handshake first (bounded, and it swallows a
    /// peer that will not answer), then the dispose that aborts whatever is left.
    /// </summary>
    private static async Task CloseSocketAsync(IYnisonSocket? socket)
    {
        if (socket is null)
        {
            return;
        }

        using var closing = new CancellationTokenSource(CloseHandshakeTimeout);
        await socket.CloseAsync(closing.Token).ConfigureAwait(false);
        await socket.DisposeAsync().ConfigureAwait(false);
    }

    private static bool IsTransient(Exception ex) => ex is WebSocketException or IOException or TimeoutException;
}
