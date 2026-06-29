namespace YandexMusic.Models;

/// <summary>A record label.</summary>
public sealed class Label
{
    /// <summary>The label identifier.</summary>
    public long Id { get; init; }

    /// <summary>The label name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The label description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>The formatted label description, when present.</summary>
    public string? DescriptionFormatted { get; init; }

    /// <summary>The label image URI template, when present.</summary>
    public string? Image { get; init; }

    /// <summary>Links to the label's external pages, when present.</summary>
    public IReadOnlyList<Link>? Links { get; init; }

    /// <summary>The label type. Known value: <c>musical</c>.</summary>
    public string? Type { get; init; }
}
