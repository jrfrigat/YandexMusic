using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>A station together with its current settings, ad parameters and presentation hints.</summary>
public sealed class StationResult
{
    /// <summary>The station descriptor.</summary>
    public Station? Station { get; init; }

    /// <summary>The legacy applied settings.</summary>
    public RotorSettings? Settings { get; init; }

    /// <summary>The current applied settings.</summary>
    public RotorSettings? Settings2 { get; init; }

    /// <summary>The advertising parameters for the station, when present.</summary>
    public AdParams? AdParams { get; init; }

    /// <summary>The explanation shown for the station, when present.</summary>
    public string? Explanation { get; init; }

    /// <summary>The pre-roll identifiers, when present.</summary>
    public IReadOnlyList<string>? Prerolls { get; init; }

    /// <summary>The "rolling up the playlist" title, when present.</summary>
    [JsonPropertyName("rupTitle")]
    public string? RupTitle { get; init; }

    /// <summary>The "rolling up the playlist" description, when present.</summary>
    [JsonPropertyName("rupDescription")]
    public string? RupDescription { get; init; }

    /// <summary>The user-assigned custom station name, when present.</summary>
    [JsonPropertyName("customName")]
    public string? CustomName { get; init; }
}

/// <summary>The currently applied tuning of a station.</summary>
public sealed class RotorSettings
{
    /// <summary>The selected language preference.</summary>
    public string Language { get; init; } = string.Empty;

    /// <summary>The selected diversity preference.</summary>
    public string Diversity { get; init; } = string.Empty;

    /// <summary>The selected mood level, when applicable.</summary>
    public int? Mood { get; init; }

    /// <summary>The selected energy level, when applicable.</summary>
    public int? Energy { get; init; }

    /// <summary>The selected combined mood/energy preference, when applicable.</summary>
    [JsonPropertyName("moodEnergy")]
    public string? MoodEnergy { get; init; }
}
