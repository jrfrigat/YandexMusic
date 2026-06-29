namespace YandexMusic.Models.MusicHistory;

/// <summary>A single entry of the body sent to resolve history item identifiers into full models.</summary>
public sealed class MusicHistoryItemRequest
{
    /// <summary>The entry type. One of <c>track</c>, <c>album</c>, <c>artist</c>, <c>playlist</c>, <c>wave</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The entry payload wrapping the identifier object.</summary>
    public MusicHistoryItemRequestData Data { get; init; } = new();
}

/// <summary>The payload of a <see cref="MusicHistoryItemRequest"/>, wrapping the per-type identifier object.</summary>
public sealed class MusicHistoryItemRequestData
{
    /// <summary>The track identifier, for a <c>track</c> entry.</summary>
    public string? TrackId { get; init; }

    /// <summary>The album the track is referenced from, for a <c>track</c> entry.</summary>
    public string? AlbumId { get; init; }

    /// <summary>The album or artist identifier, for an <c>album</c> or <c>artist</c> entry.</summary>
    public string? Id { get; init; }

    /// <summary>The playlist owner identifier, for a <c>playlist</c> entry.</summary>
    public long? Uid { get; init; }

    /// <summary>The playlist kind, for a <c>playlist</c> entry.</summary>
    public long? Kind { get; init; }

    /// <summary>The wave seeds, for a <c>wave</c> entry.</summary>
    public IReadOnlyList<string>? Seeds { get; init; }
}
