using System.Text.Json;

namespace YandexMusic.Models.Account;

/// <summary>The configuration of a single A/B experiment the account participates in.</summary>
public sealed class ExperimentDetail
{
    /// <summary>The experiment group the account is assigned to, when present.</summary>
    public string? Group { get; init; }

    /// <summary>The experiment-specific value payload, when present.</summary>
    public ExperimentDetailValue? Value { get; init; }
}

/// <summary>
/// The value payload of an experiment. Aside from the well-known <see cref="Title"/> slot the keys are
/// experiment-specific, so any additional members are captured into <see cref="AdditionalProperties"/>.
/// </summary>
public sealed class ExperimentDetailValue
{
    /// <summary>The title, when present. Mirrors the parent experiment group.</summary>
    public string? Title { get; init; }

    /// <summary>The remaining experiment-specific configuration keys.</summary>
    [System.Text.Json.Serialization.JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
