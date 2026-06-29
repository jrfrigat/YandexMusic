namespace YandexMusic.Models.Clips;

/// <summary>A page of recommended clips the user is likely to enjoy.</summary>
public sealed class ClipsWillLike
{
    /// <summary>The recommended clips.</summary>
    public IReadOnlyList<Clip>? Clips { get; init; }

    /// <summary>The pagination information, when present.</summary>
    public Pager? Pager { get; init; }
}
