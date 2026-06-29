namespace YandexMusic.Models.Artists;

/// <summary>
/// The placeholder layout (skeleton) of an artist landing page: an ordered list of blocks that
/// describe which sections to render and in what order.
/// </summary>
public sealed class ArtistSkeleton
{
    /// <summary>The skeleton identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The skeleton title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The blocks that make up the page.</summary>
    public IReadOnlyList<SkeletonBlock>? Blocks { get; init; }
}
