using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>The radio landing dashboard: a curated set of recommended stations.</summary>
public sealed class Dashboard
{
    /// <summary>The dashboard identifier.</summary>
    [JsonPropertyName("dashboardId")]
    public string DashboardId { get; init; } = string.Empty;

    /// <summary>The recommended stations.</summary>
    public IReadOnlyList<StationResult> Stations { get; init; } = [];

    /// <summary>Whether the seasonal "pumpkin" presentation is active.</summary>
    public bool Pumpkin { get; init; }
}
