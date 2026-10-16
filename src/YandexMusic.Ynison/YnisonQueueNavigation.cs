namespace YandexMusic.Ynison;

/// <summary>
/// Reads a Ynison queue in playback order. With shuffle on, the order of
/// <see cref="PlayerQueue.PlayableList"/> is not the order tracks are played in: the shuffled order is
/// <see cref="Shuffle.PlayableIndices"/>, and "next" and "previous" have to follow it.
/// </summary>
public static class YnisonQueueNavigation
{
    /// <summary>Gets the indices of <see cref="PlayerQueue.PlayableList"/> in the order they are played.</summary>
    /// <param name="queue">The queue.</param>
    /// <returns>The shuffled order when shuffle is on and consistent with the list, otherwise the list order.</returns>
    public static IReadOnlyList<int> GetPlaybackOrder(PlayerQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var total = queue.PlayableList.Count;
        var shuffled = queue.ShuffleOptional?.PlayableIndices;

        // A shuffle order that does not cover the list exactly is stale; trusting it would skip or
        // repeat tracks, so the plain order is the safer reading.
        if (shuffled is { Count: > 0 } && shuffled.Count == total)
        {
            return shuffled;
        }

        var order = new int[total];
        for (var i = 0; i < total; i++)
        {
            order[i] = i;
        }

        return order;
    }

    /// <summary>
    /// Gets the index of the track <paramref name="delta"/> steps away from the current one in
    /// playback order. With <see cref="RepeatMode.All"/> the queue wraps around at either end.
    /// </summary>
    /// <param name="queue">The queue.</param>
    /// <param name="delta">The shift in playback order: 1 for the next track, -1 for the previous one.</param>
    /// <returns>
    /// The index in <see cref="PlayerQueue.PlayableList"/>, or <see langword="null"/> when the shift
    /// leaves the queue or the current track is unknown.
    /// </returns>
    public static int? GetNeighbourIndex(PlayerQueue queue, int delta)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var order = GetPlaybackOrder(queue);
        if (order.Count == 0)
        {
            return null;
        }

        var position = -1;
        for (var i = 0; i < order.Count; i++)
        {
            if (order[i] == queue.CurrentPlayableIndex)
            {
                position = i;
                break;
            }
        }

        if (position < 0)
        {
            return null;
        }

        var target = position + delta;
        if (queue.Options?.RepeatMode == RepeatMode.All)
        {
            target = ((target % order.Count) + order.Count) % order.Count;
        }

        return target >= 0 && target < order.Count ? order[target] : null;
    }

    /// <summary>Gets the track currently selected in the queue.</summary>
    /// <param name="queue">The queue.</param>
    /// <returns>The current playable, or <see langword="null"/> when the queue is empty or the index is out of range.</returns>
    public static Playable? GetCurrentPlayable(PlayerQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var index = queue.CurrentPlayableIndex;
        return index >= 0 && index < queue.PlayableList.Count ? queue.PlayableList[index] : null;
    }
}
