namespace YandexMusic.Models.Concerts;

/// <summary>The locations available for filtering the concert feed.</summary>
public sealed class ConcertLocations
{
    /// <summary>The available locations.</summary>
    public IReadOnlyList<ConcertLocation> Locations { get; init; } = [];
}

/// <summary>A single location (geo region) available for the concert feed.</summary>
public sealed class ConcertLocation
{
    /// <summary>The geo identifier, when present.</summary>
    public long? Id { get; init; }

    /// <summary>The location name, when present.</summary>
    public string? Name { get; init; }
}
