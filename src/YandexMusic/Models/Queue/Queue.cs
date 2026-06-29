using System.Text.Json.Serialization;

namespace YandexMusic.Models.Queue;

/// <summary>
/// A personal listening queue used for cross-device synchronization. It pairs the source the queue
/// was built from with the ordered track references and the index currently being played.
/// </summary>
public sealed class Queue
{
    /// <summary>The source the queue was built from, when known.</summary>
    public Context? Context { get; init; }

    /// <summary>The ordered track references that make up the queue.</summary>
    public IReadOnlyList<TrackId> Tracks { get; init; } = [];

    /// <summary>The index of the track currently being played.</summary>
    public int CurrentIndex { get; init; }

    /// <summary>The last-modified timestamp reported by the server.</summary>
    public string Modified { get; init; } = string.Empty;

    /// <summary>The unique queue identifier, when assigned.</summary>
    public string? Id { get; init; }

    /// <summary>The source the queue originated from, when provided.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }
}
