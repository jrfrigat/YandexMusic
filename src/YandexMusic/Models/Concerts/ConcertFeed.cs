namespace YandexMusic.Models.Concerts;

/// <summary>A feed of concerts, optionally filtered by location.</summary>
public sealed class ConcertFeed
{
    /// <summary>The feed items.</summary>
    public IReadOnlyList<ConcertFeedItem> Items { get; init; } = [];
}

/// <summary>A single entry within a <see cref="ConcertFeed"/>.</summary>
public sealed class ConcertFeedItem
{
    /// <summary>The item type. The known value is <c>concert_item</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The item payload, when present.</summary>
    public ConcertFeedItemData? Data { get; init; }
}

/// <summary>The payload of a <see cref="ConcertFeedItem"/>.</summary>
public sealed class ConcertFeedItemData
{
    /// <summary>The concert, when present.</summary>
    public Concert? Concert { get; init; }

    /// <summary>The lowest available ticket price, when present.</summary>
    public ConcertMinPrice? MinPrice { get; init; }
}
