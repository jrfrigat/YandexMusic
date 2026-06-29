using YandexMusic.Models;

namespace YandexMusic.Models.Concerts;

/// <summary>A single concert (afisha) event in the catalogue.</summary>
public sealed class Concert
{
    /// <summary>The concert identifier (a UUID), when present.</summary>
    public string? Id { get; init; }

    /// <summary>The poster image URI templates, when present.</summary>
    public IReadOnlyList<string>? Images { get; init; }

    /// <summary>The primary image URL, when present.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>The concert title, when present.</summary>
    public string? ConcertTitle { get; init; }

    /// <summary>The external afisha (ticket page) URL, when present.</summary>
    public string? AfishaUrl { get; init; }

    /// <summary>The city where the concert takes place, when present.</summary>
    public string? City { get; init; }

    /// <summary>The venue name, when present.</summary>
    public string? Place { get; init; }

    /// <summary>The venue address, when present.</summary>
    public string? Address { get; init; }

    /// <summary>The concert start time as an ISO 8601 string, when present.</summary>
    public string? Datetime { get; init; }

    /// <summary>The age content rating (for example <c>16+</c>), when present.</summary>
    public string? ContentRating { get; init; }

    /// <summary>The lowest available ticket price, when present.</summary>
    public ConcertMinPrice? MinPrice { get; init; }

    /// <summary>The cashback offer for the concert, when present.</summary>
    public ConcertCashback? Cashback { get; init; }

    /// <summary>Additional event classification, when present.</summary>
    public ConcertEventInfo? EventInfo { get; init; }

    /// <summary>The concert cover image, when present.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The data session identifier used for analytics, when present.</summary>
    public string? DataSessionId { get; init; }
}

/// <summary>The lowest available ticket price for a concert.</summary>
public sealed class ConcertMinPrice
{
    /// <summary>The price amount, when present.</summary>
    public int? Value { get; init; }

    /// <summary>The ISO currency code (for example <c>RUB</c>), when present.</summary>
    public string? Currency { get; init; }

    /// <summary>The currency display symbol (for example <c>₽</c>), when present.</summary>
    public string? CurrencySymbol { get; init; }
}

/// <summary>A cashback offer attached to a concert.</summary>
public sealed class ConcertCashback
{
    /// <summary>The offer title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The cashback percentage, when present.</summary>
    public int? ValuePercent { get; init; }
}

/// <summary>Additional classification of a concert event.</summary>
public sealed class ConcertEventInfo
{
    /// <summary>The event type. Known values include <c>concert</c> and <c>festival</c>.</summary>
    public string? Type { get; init; }
}
