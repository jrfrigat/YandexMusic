namespace YandexMusic.Models;

/// <summary>A link to an external page of an artist or label (an official site or social network).</summary>
public sealed class Link
{
    /// <summary>The display name of the page.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The URL of the page.</summary>
    public string Href { get; init; } = string.Empty;

    /// <summary>The kind of page. Known values: <c>official</c>, <c>social</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The social network name, when <see cref="Type"/> is <c>social</c>.</summary>
    public string? SocialNetwork { get; init; }
}
