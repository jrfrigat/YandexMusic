namespace YandexMusic.Models.Queue;

/// <summary>The envelope the queues listing wraps its items in (<c>{ "queues": [ … ] }</c>).</summary>
internal sealed class QueuesListResult
{
    /// <summary>The queues listed across the user's devices.</summary>
    public IReadOnlyList<QueueItem> Queues { get; init; } = [];
}

/// <summary>The payload returned when creating a queue (<c>{ "id": "…" }</c>).</summary>
internal sealed class QueueCreateResult
{
    /// <summary>The identifier assigned to the newly created queue.</summary>
    public string? Id { get; init; }
}

/// <summary>The payload returned by the update-position endpoint (<c>{ "status": "ok" }</c>).</summary>
internal sealed class QueueUpdateResult
{
    /// <summary>The operation status, <c>ok</c> on success.</summary>
    public string? Status { get; init; }
}
