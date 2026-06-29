using System.Text.Json.Serialization;

namespace YandexMusic.Models.Playlists;

/// <summary>A single operation in a playlist <c>diff</c> (insert or delete a range of tracks).</summary>
internal sealed class PlaylistDiffOperation
{
    /// <summary>The operation kind. Known values: <c>insert</c>, <c>delete</c>.</summary>
    [JsonPropertyName("op")]
    public string Op { get; init; } = string.Empty;

    /// <summary>The insertion index (for <c>insert</c>).</summary>
    [JsonPropertyName("at")]
    public int? At { get; init; }

    /// <summary>The first index to remove (for <c>delete</c>).</summary>
    [JsonPropertyName("from")]
    public int? From { get; init; }

    /// <summary>The exclusive end index to remove (for <c>delete</c>).</summary>
    [JsonPropertyName("to")]
    public int? To { get; init; }

    /// <summary>The tracks to insert (for <c>insert</c>).</summary>
    [JsonPropertyName("tracks")]
    public IReadOnlyList<PlaylistDiffTrack>? Tracks { get; init; }
}

/// <summary>A track reference inside a playlist <c>diff</c> insert operation.</summary>
internal sealed class PlaylistDiffTrack
{
    /// <summary>The track identifier.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The album identifier the track is taken from.</summary>
    [JsonPropertyName("album_id")]
    public string AlbumId { get; init; } = string.Empty;
}
