using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace YandexMusic.Ynison;

/// <summary>A single Ynison websocket connection. Internal seam so tests can script the transport.</summary>
internal interface IYnisonSocket : IAsyncDisposable
{
    /// <summary>Opens the connection to <paramref name="uri"/> with the given subprotocols and headers.</summary>
    /// <param name="uri">The websocket URI.</param>
    /// <param name="subprotocols">The subprotocols to negotiate (Bearer, version, device info).</param>
    /// <param name="headers">The extra request headers (Origin, Authorization).</param>
    /// <param name="keepAliveInterval">The client ping interval.</param>
    /// <param name="cancellationToken">A token to cancel the connect.</param>
    Task ConnectAsync(
        Uri uri,
        IReadOnlyList<string> subprotocols,
        IReadOnlyDictionary<string, string> headers,
        TimeSpan keepAliveInterval,
        CancellationToken cancellationToken);

    /// <summary>Receives the next text frame, skipping binary frames.</summary>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The frame text, or <see langword="null"/> when the connection closed.</returns>
    Task<string?> ReceiveTextAsync(CancellationToken cancellationToken);

    /// <summary>Sends a text frame.</summary>
    /// <param name="message">The frame text.</param>
    /// <param name="cancellationToken">A token to cancel the send.</param>
    Task SendAsync(string message, CancellationToken cancellationToken);

    /// <summary>Closes the connection gracefully.</summary>
    /// <param name="cancellationToken">A token to cancel the close handshake.</param>
    Task CloseAsync(CancellationToken cancellationToken);
}

/// <summary>Creates <see cref="IYnisonSocket"/> connections.</summary>
internal interface IYnisonSocketFactory
{
    /// <summary>Creates a not-yet-connected socket.</summary>
    /// <returns>The socket.</returns>
    IYnisonSocket Create();
}

/// <summary>The default transport over <see cref="ClientWebSocket"/>.</summary>
internal sealed class ClientWebSocketYnisonSocketFactory : IYnisonSocketFactory
{
    /// <inheritdoc />
    public IYnisonSocket Create() => new ClientWebSocketYnisonSocket();

    private sealed class ClientWebSocketYnisonSocket : IYnisonSocket
    {
        private ClientWebSocket? _socket;

        public async Task ConnectAsync(
            Uri uri,
            IReadOnlyList<string> subprotocols,
            IReadOnlyDictionary<string, string> headers,
            TimeSpan keepAliveInterval,
            CancellationToken cancellationToken)
        {
            var socket = new ClientWebSocket();
            foreach (var subprotocol in subprotocols)
            {
                socket.Options.AddSubProtocol(subprotocol);
            }

            foreach (var header in headers)
            {
                socket.Options.SetRequestHeader(header.Key, header.Value);
            }

            socket.Options.KeepAliveInterval = keepAliveInterval;

            await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
            _socket = socket;
        }

        public async Task<string?> ReceiveTextAsync(CancellationToken cancellationToken)
        {
            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            var buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
            try
            {
                while (true)
                {
                    using var message = new MemoryStream();
                    var isBinary = false;
                    ValueWebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(
                            buffer.AsMemory(0, buffer.Length),
                            cancellationToken).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            return null;
                        }

                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            // The protocol is text-only; drain the unexpected frame without buffering it.
                            isBinary = true;
                        }
                        else if (!isBinary)
                        {
                            message.Write(buffer, 0, result.Count);
                        }
                    }
                    while (!result.EndOfMessage);

                    if (!isBinary)
                    {
                        var bytes = message.GetBuffer();
                        return Encoding.UTF8.GetString(bytes, 0, checked((int)message.Length));
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public async Task SendAsync(string message, CancellationToken cancellationToken)
        {
            var socket = _socket ?? throw new InvalidOperationException("The socket is not connected.");
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            var socket = _socket;
            if (socket is null)
            {
                return;
            }

            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client stopping.", cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is WebSocketException or IOException or OperationCanceledException)
            {
                // A closing handshake that the peer refuses must not fail the shutdown path.
            }
        }

        public async ValueTask DisposeAsync()
        {
            var socket = _socket;
            _socket = null;
            if (socket is not null)
            {
                // Abort, so a pending ReceiveAsync on another thread unblocks immediately.
                socket.Abort();
                socket.Dispose();
            }

            await ValueTask.CompletedTask.ConfigureAwait(false);
        }
    }
}
