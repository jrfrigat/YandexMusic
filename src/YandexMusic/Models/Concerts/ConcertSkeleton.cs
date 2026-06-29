using YandexMusic.Models;

namespace YandexMusic.Models.Concerts;

/// <summary>The placeholder layout (skeleton) of a concert page.</summary>
public sealed class ConcertSkeleton
{
    /// <summary>The skeleton identifier, when present.</summary>
    public string? Id { get; init; }

    /// <summary>The page title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The structural blocks of the page, when present.</summary>
    public IReadOnlyList<SkeletonBlock>? Blocks { get; init; }
}
