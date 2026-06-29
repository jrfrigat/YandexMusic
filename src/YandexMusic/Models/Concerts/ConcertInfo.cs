using YandexMusic.Models;

namespace YandexMusic.Models.Concerts;

/// <summary>Detailed information about a single concert.</summary>
public sealed class ConcertInfo
{
    /// <summary>The concert, when present.</summary>
    public Concert? Concert { get; init; }

    /// <summary>The lowest available ticket price, when present.</summary>
    public ConcertMinPrice? MinPrice { get; init; }

    /// <summary>The concert cover images.</summary>
    public IReadOnlyList<Cover> Covers { get; init; } = [];

    /// <summary>The concert description, when present.</summary>
    public ConcertDescription? Description { get; init; }

    /// <summary>The identifier of the lead artist, when present.</summary>
    public long? LeadArtistId { get; init; }
}

/// <summary>A textual description of a concert, with its source.</summary>
public sealed class ConcertDescription
{
    /// <summary>The description text, when present.</summary>
    public string? Text { get; init; }

    /// <summary>The source of the description, when present.</summary>
    public string? Source { get; init; }
}
