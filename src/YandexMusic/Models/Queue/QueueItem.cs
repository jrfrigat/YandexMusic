namespace YandexMusic.Models.Queue;

/// <summary>A lightweight queue summary as listed across a user's devices.</summary>
public sealed class QueueItem
{
    /// <summary>The unique queue identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The source the queue was built from, when known.</summary>
    public Context? Context { get; init; }

    /// <summary>The last-modified timestamp reported by the server.</summary>
    public string Modified { get; init; } = string.Empty;
}
