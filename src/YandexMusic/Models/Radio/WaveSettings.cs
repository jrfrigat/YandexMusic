using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>
/// The settings of the personal wave: the contexts (activities) it can be tuned to and the values
/// each setting accepts.
/// </summary>
/// <remarks>
/// The values in <see cref="SettingRestrictions"/> double as session seeds: each
/// <see cref="EnumValue.SerializedSeed"/> (for example <c>settingDiversity:favorite</c>) can be
/// passed straight to a session request, which is how a tuned wave is started.
/// </remarks>
public sealed class WaveSettings
{
    /// <summary>The station the wave falls back to.</summary>
    [JsonPropertyName("defaultStation")]
    public WaveDefaultStation? DefaultStation { get; init; }

    /// <summary>The blocks of tunable settings, for example the list of activity contexts.</summary>
    public IReadOnlyList<WaveSettingsBlock>? Blocks { get; init; }

    /// <summary>The values each wave setting accepts.</summary>
    [JsonPropertyName("settingRestrictions")]
    public Restrictions? SettingRestrictions { get; init; }
}

/// <summary>The station the wave falls back to when no other one is chosen.</summary>
public sealed class WaveDefaultStation
{
    /// <summary>The station identity, for example <c>user:12345</c>.</summary>
    [JsonPropertyName("stationId")]
    public string? StationId { get; init; }

    /// <summary>The station name.</summary>
    public string? Title { get; init; }

    /// <summary>The station headline.</summary>
    [JsonPropertyName("rupTitle")]
    public string? RupTitle { get; init; }

    /// <summary>The station description.</summary>
    [JsonPropertyName("rupDescription")]
    public string? RupDescription { get; init; }
}

/// <summary>A block of wave settings: a group of stations offered as one tunable dimension.</summary>
public sealed class WaveSettingsBlock
{
    /// <summary>The block type. The known value is <c>contexts</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The stations the block offers, for example the activity contexts.</summary>
    public IReadOnlyList<Station>? Items { get; init; }
}
