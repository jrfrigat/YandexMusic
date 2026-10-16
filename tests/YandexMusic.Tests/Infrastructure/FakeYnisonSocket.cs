using YandexMusic.Ynison;
using Xunit;

namespace YandexMusic.Tests.Infrastructure;

/// <summary>Hands out scripted <see cref="FakeSocket"/> instances in order, one per connection attempt.</summary>
internal sealed class FakeSocketFactory(IReadOnlyList<FakeSocket> sockets) : IYnisonSocketFactory
{
    private int _created;

    /// <summary>How many sockets the client asked for so far.</summary>
    public int Created => _created;

    public IYnisonSocket Create()
    {
        Assert.InRange(_created, 0, sockets.Count - 1);
        return sockets[_created++];
    }
}

/// <summary>
/// A scripted Ynison websocket: it delivers the given frames in order (<see langword="null"/> stands
/// for a graceful close), records what the client sends, and parks once the script runs out.
/// </summary>
internal sealed class FakeSocket(IEnumerable<string?> incoming) : IYnisonSocket
{
    private readonly Queue<string?> _incoming = new(incoming);
    private readonly TaskCompletionSource _parked = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _disposed;

    public Uri? Uri { get; private set; }

    public List<string>? Subprotocols { get; private set; }

    public Dictionary<string, string>? Headers { get; private set; }

    public TimeSpan KeepAliveInterval { get; private set; }

    public List<string> Sent { get; } = [];

    /// <summary>Set to make the next send fail, standing in for a socket that dropped.</summary>
    public Exception? SendFailure { get; set; }

    /// <summary>How many times a close handshake was requested.</summary>
    public int Closes { get; private set; }

    public Task ConnectAsync(
        Uri uri,
        IReadOnlyList<string> subprotocols,
        IReadOnlyDictionary<string, string> headers,
        TimeSpan keepAliveInterval,
        CancellationToken cancellationToken)
    {
        Uri = uri;
        Subprotocols = [.. subprotocols];
        Headers = new Dictionary<string, string>(headers);
        KeepAliveInterval = keepAliveInterval;
        return Task.CompletedTask;
    }

    public async Task<string?> ReceiveTextAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (_incoming.TryDequeue(out var frame))
            {
                return frame;
            }

            await _parked.Task.WaitAsync(cancellationToken);
        }
    }

    public Task SendAsync(string message, CancellationToken cancellationToken)
    {
        if (SendFailure is not null)
        {
            throw SendFailure;
        }

        Sent.Add(message);
        return Task.CompletedTask;
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        Closes++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return ValueTask.CompletedTask;
        }

        // Unblock a parked receive; the cancellation surfaces to the client's stop path.
        _parked.TrySetCanceled();
        return ValueTask.CompletedTask;
    }
}
