using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using YandexMusic.Exceptions;

namespace YandexMusic.Quasar.Control;

/// <summary>
/// The default <see cref="ILocalDeviceControl"/>: a websocket to the speaker itself, over TLS whose
/// certificate is pinned to the one the Quasar backend published for that device.
///
/// Two things about this connection are not obvious and both were learned the hard way. The proxy
/// has to be disabled explicitly, because a configured system proxy otherwise swallows connections
/// to the local network. And the answer to a command carries the state from <b>before</b> the
/// command was applied, so nothing here treats a reply as confirmation — the state stream is what
/// tells you what the device is doing.
/// </summary>
public sealed class LocalDeviceControl : ILocalDeviceControl
{
    private readonly Uri _endpoint;
    private readonly string _deviceToken;
    private readonly X509Certificate2? _expectedCertificate;
    private readonly TaskCompletionSource _firstState = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private ClientWebSocket? _socket;
    private LocalDeviceFrame? _latestState;
    private int _disposed;

    /// <summary>Creates a connection to a device.</summary>
    /// <param name="deviceId">The device's identifier, used for diagnostics.</param>
    /// <param name="endpoint">Where the device serves local control.</param>
    /// <param name="deviceToken">The per-device token from <see cref="IQuasarClient.GetDeviceTokenAsync"/>.</param>
    /// <param name="expectedCertificate">
    /// The certificate the device must present, from the Quasar backend. When <see langword="null"/>
    /// the device's certificate is accepted unverified — it is self-signed and names <c>localhost</c>,
    /// so ordinary validation can never succeed. Passing the expected certificate is the only way to
    /// know you are talking to the right speaker; leaving it out is a deliberate choice to not know.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deviceId"/> or <paramref name="deviceToken"/> is null or whitespace.</exception>
    public LocalDeviceControl(string deviceId, IPEndPoint endpoint, string deviceToken, X509Certificate2? expectedCertificate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceToken);

        DeviceId = deviceId;
        _endpoint = new Uri($"wss://{endpoint.Address}:{endpoint.Port}");
        _deviceToken = deviceToken;
        _expectedCertificate = expectedCertificate;
    }

    /// <inheritdoc />
    public event EventHandler<LocalDeviceFrame>? StateReceived;

    /// <inheritdoc />
    public event EventHandler<Exception>? ListenerError;

    /// <inheritdoc />
    public event EventHandler<string>? FrameReceived;

    /// <inheritdoc />
    public event EventHandler<string>? FrameSent;

    /// <inheritdoc />
    public string DeviceId { get; }

    /// <inheritdoc />
    public LocalDeviceFrame? LatestState => Volatile.Read(ref _latestState);

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var socket = new ClientWebSocket();
        // A LAN address must never be tunnelled: with a system proxy configured, the default
        // behaviour turns this into a CONNECT the proxy refuses, long before TLS is attempted.
        socket.Options.Proxy = null;
        socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
        socket.Options.RemoteCertificateValidationCallback = ValidateCertificate;
        _socket = socket;

        try
        {
            await socket.ConnectAsync(_endpoint, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is WebSocketException or HttpRequestException)
        {
            _ = _firstState.TrySetException(Failure($"Could not connect to device '{DeviceId}' at {_endpoint}.", exception));
            throw Failure($"Could not connect to device '{DeviceId}' at {_endpoint}.", exception);
        }

        // The device says nothing at all until it is spoken to; this opens the conversation.
        await SendAsync(new GlagolPayload("ping"), cancellationToken).ConfigureAwait(false);

        try
        {
            await ReceiveLoopAsync(socket, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _firstState.TrySetException(new YandexMusicQuasarException(
                $"The connection to device '{DeviceId}' ended before any state arrived."));
        }
    }

    /// <inheritdoc />
    public async Task<LocalDeviceFrame> WaitForStateAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        try
        {
            await _firstState.Task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException exception)
        {
            throw new YandexMusicQuasarException($"Device '{DeviceId}' sent no state within {timeout}.", exception);
        }

        return LatestState ?? throw new YandexMusicQuasarException($"Device '{DeviceId}' sent no usable state.");
    }

    /// <inheritdoc />
    public Task PingAsync(CancellationToken cancellationToken = default)
        => SendAsync(new GlagolPayload("ping"), cancellationToken);

    /// <inheritdoc />
    public Task PlayAsync(CancellationToken cancellationToken = default)
        => SendAsync(new GlagolPayload("play"), cancellationToken);

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken = default)
        => SendAsync(new GlagolPayload("stop"), cancellationToken);

    /// <inheritdoc />
    public Task NextTrackAsync(CancellationToken cancellationToken = default)
        => SendAsync(new GlagolPayload("next"), cancellationToken);

    /// <inheritdoc />
    public Task PreviousTrackAsync(CancellationToken cancellationToken = default)
        => SendAsync(new GlagolPayload("prev"), cancellationToken);

    /// <inheritdoc />
    public Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default)
    {
        if (volume is < 0 or > 1 || double.IsNaN(volume))
        {
            throw new ArgumentOutOfRangeException(nameof(volume), volume, "The volume must be between 0 and 1.");
        }

        return SendAsync(new GlagolPayload("setVolume") { Volume = volume }, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var socket = Interlocked.Exchange(ref _socket, null);
        if (socket is null)
        {
            return;
        }

        try
        {
            if (socket.State == WebSocketState.Open)
            {
                using var closing = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, closing.Token).ConfigureAwait(false);
            }
        }
        catch (Exception exception) when (exception is WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            // The point of closing politely is to be polite; the socket is going away regardless.
        }
        finally
        {
            socket.Dispose();
        }
    }

    private bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
    {
        if (_expectedCertificate is null)
        {
            // No expectation was supplied, so there is nothing to compare against. Ordinary validation
            // cannot succeed here either: the certificate is self-signed and says CN=localhost.
            return true;
        }

        // Compare the certificate itself rather than validating it. Dates are deliberately not
        // checked: speakers are shipping certificates that expired years ago and still work, and
        // rejecting them would lock out the real device while proving nothing about any other.
        return certificate is not null &&
            certificate.GetCertHashString().Equals(_expectedCertificate.GetCertHashString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task SendAsync(GlagolPayload payload, CancellationToken cancellationToken)
    {
        var socket = Volatile.Read(ref _socket);
        if (socket is null || socket.State != WebSocketState.Open)
        {
            throw new YandexMusicQuasarException($"Not connected to device '{DeviceId}'.");
        }

        var request = new GlagolRequest(
            _deviceToken,
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            payload);

        var json = JsonSerializer.SerializeToUtf8Bytes(request, GlagolJson.TypeInfo<GlagolRequest>());
        if (FrameSent is { } sent)
        {
            Announce(sent, JsonSerializer.Serialize(
                request with { ConversationToken = "<redacted>" }, GlagolJson.TypeInfo<GlagolRequest>()));
        }

        try
        {
            await socket.SendAsync(json, WebSocketMessageType.Text, endOfMessage: true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is WebSocketException or ObjectDisposedException or InvalidOperationException)
        {
            throw Failure($"Could not send '{payload.Command}' to device '{DeviceId}'.", exception);
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        var message = new ArrayBufferWriter<byte>();

        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is WebSocketException or ObjectDisposedException or OperationCanceledException)
            {
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return;
            }

            message.Write(buffer.AsSpan(0, result.Count));
            if (!result.EndOfMessage)
            {
                continue;
            }

            Absorb(message.WrittenSpan);
            message.Clear();
        }
    }

    /// <summary>Raises a diagnostic event without letting a listener's failure reach the caller.</summary>
    private void Announce(EventHandler<string>? handler, string text)
    {
        try
        {
            handler?.Invoke(this, text);
        }
        catch (Exception)
        {
            // A diagnostic hook must never be able to break the thing it is observing.
        }
    }

    private void Absorb(ReadOnlySpan<byte> payload)
    {
        // These frames are large and arrive on every change, so the text is only built when there is
        // somebody to hand it to.
        if (FrameReceived is { } received)
        {
            Announce(received, Encoding.UTF8.GetString(payload));
        }

        LocalDeviceFrame? frame;
        try
        {
            frame = JsonSerializer.Deserialize(payload, GlagolJson.TypeInfo<LocalDeviceFrame>());
        }
        catch (JsonException)
        {
            // A frame this client cannot read is not a reason to drop a working connection.
            return;
        }

        if (frame is null)
        {
            return;
        }

        Volatile.Write(ref _latestState, frame);
        _ = _firstState.TrySetResult();

        try
        {
            StateReceived?.Invoke(this, frame);
        }
        catch (Exception exception)
        {
            // A listener's failure is the listener's problem; isolate it so it cannot stop the loop.
            try
            {
                ListenerError?.Invoke(this, exception);
            }
            catch (Exception)
            {
                // Nothing sensible remains to be done about a failing error handler.
            }
        }
    }

    private static YandexMusicQuasarException Failure(string message, Exception inner)
        => new(message, inner);
}
