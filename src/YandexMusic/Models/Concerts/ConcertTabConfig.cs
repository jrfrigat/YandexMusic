namespace YandexMusic.Models.Concerts;

/// <summary>The tab and pagination configuration for the concerts feed.</summary>
public sealed class ConcertTabConfig
{
    /// <summary>The configuration payload, when present.</summary>
    public ConcertTabConfigData? Config { get; init; }
}

/// <summary>The payload of a <see cref="ConcertTabConfig"/>.</summary>
public sealed class ConcertTabConfigData
{
    /// <summary>The range used for the "top" section, when present.</summary>
    public ConcertTabRange? Top { get; init; }

    /// <summary>The range used for the main feed section, when present.</summary>
    public ConcertTabRange? Feed { get; init; }
}

/// <summary>An offset/limit range describing how many items a feed section shows.</summary>
public sealed class ConcertTabRange
{
    /// <summary>The starting offset, when present.</summary>
    public int? Offset { get; init; }

    /// <summary>The item limit, when present. A value of <c>-1</c> means "to the end".</summary>
    public int? Limit { get; init; }
}
