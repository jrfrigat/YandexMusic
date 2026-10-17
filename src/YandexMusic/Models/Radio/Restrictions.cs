using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>The tunable parameters of a station and the values each parameter accepts.</summary>
public sealed class Restrictions
{
    /// <summary>The language restriction, expressed as a set of choices.</summary>
    public StationEnum? Language { get; init; }

    /// <summary>The diversity restriction, expressed as a set of choices.</summary>
    public StationEnum? Diversity { get; init; }

    /// <summary>The mood restriction, expressed as a discrete scale.</summary>
    public DiscreteScale? Mood { get; init; }

    /// <summary>The energy restriction, expressed as a discrete scale.</summary>
    public DiscreteScale? Energy { get; init; }

    /// <summary>The combined mood/energy restriction, expressed as a set of choices.</summary>
    [JsonPropertyName("moodEnergy")]
    public StationEnum? MoodEnergy { get; init; }
}

/// <summary>A restriction modeled as a closed set of selectable values.</summary>
public sealed class StationEnum
{
    /// <summary>The restriction kind. The wire value is <c>enum</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The restriction name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The values that may be selected for this restriction.</summary>
    [JsonPropertyName("possibleValues")]
    public IReadOnlyList<EnumValue> PossibleValues { get; init; } = [];
}

/// <summary>A restriction modeled as a discrete scale between a minimum and a maximum value.</summary>
public sealed class DiscreteScale
{
    /// <summary>The restriction kind. The wire value is <c>discrete-scale</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The restriction name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The lower bound of the scale, when provided.</summary>
    public EnumValue? Min { get; init; }

    /// <summary>The upper bound of the scale, when provided.</summary>
    public EnumValue? Max { get; init; }
}

/// <summary>A single selectable value of a restriction: its wire value and its display name.</summary>
public sealed class EnumValue
{
    /// <summary>
    /// The value sent back to the API when this option is chosen. The bounds of a discrete scale
    /// arrive as JSON numbers (the wave settings send them that way), so the value is normalized to
    /// a string.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Value { get; init; } = string.Empty;

    /// <summary>The human-readable name of the value.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The illustration of the value, when the server provides one.</summary>
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    /// <summary>
    /// The value expressed as a radio seed, for example <c>settingDiversity:favorite</c>. Passing it
    /// as a seed is how a session is started with this setting already applied, which replaces
    /// changing the station settings first.
    /// </summary>
    [JsonPropertyName("serializedSeed")]
    public string? SerializedSeed { get; init; }

    /// <summary>Whether this is the catch-all default ("any") rather than a specific choice.</summary>
    public bool? Unspecified { get; init; }
}
