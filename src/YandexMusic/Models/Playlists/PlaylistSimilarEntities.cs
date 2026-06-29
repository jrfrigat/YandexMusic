namespace YandexMusic.Models.Playlists;

/// <summary>The "similar entities" (wave-agent recommendations) returned for a playlist.</summary>
public sealed class PlaylistSimilarEntities
{
    /// <summary>The recommended items, when present.</summary>
    public IReadOnlyList<SimilarEntityItem>? Items { get; init; }
}
