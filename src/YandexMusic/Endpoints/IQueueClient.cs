using YandexMusic.Models.Queue;

namespace YandexMusic.Endpoints;

/// <summary>
/// Endpoints for personal listening queues under <c>/queues/*</c>, used to synchronize playback
/// across a user's devices. All of these endpoints require authentication.
/// </summary>
public interface IQueueClient
{
    /// <summary>Lists the track queues from all of the user's devices for cross-device synchronization.</summary>
    /// <param name="device">The device descriptor (<c>key=value; key2=value2</c>); a default is used when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The queues across all devices, or an empty list when none exist.</returns>
    Task<IReadOnlyList<QueueItem>> GetQueuesAsync(string? device = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a single queue together with its track references.</summary>
    /// <param name="queueId">The unique queue identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The queue, or <see langword="null"/> when it does not exist.</returns>
    Task<Queue?> GetAsync(string queueId, CancellationToken cancellationToken = default);

    /// <summary>Sets the index of the track currently being played in a queue.</summary>
    /// <param name="queueId">The unique queue identifier.</param>
    /// <param name="currentIndex">The index of the track currently being played.</param>
    /// <param name="device">The device the queue was created with; must match. A default is used when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the update.</returns>
    Task<bool> UpdatePositionAsync(string queueId, int currentIndex, string? device = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new track queue bound to a device.</summary>
    /// <param name="queue">The queue to create.</param>
    /// <param name="device">The device to bind the queue to; a default is used when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The identifier of the newly created queue, or <see langword="null"/> when none was returned.</returns>
    Task<string?> CreateAsync(Queue queue, string? device = null, CancellationToken cancellationToken = default);
}
