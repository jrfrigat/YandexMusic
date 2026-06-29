namespace YandexMusic.Models.Albums;

/// <summary>The "similar entities" response for an album: a list of wave-agent recommendations.</summary>
public sealed class AlbumSimilarEntities
{
    /// <summary>The recommended similar entities, when present.</summary>
    public IReadOnlyList<SimilarEntityItem>? Items { get; init; }
}
