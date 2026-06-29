using System.Text.Json.Serialization;

namespace YandexMusic.Models;

/// <summary>A "wave" radio seed: the description of a personalized stream built from an entity.</summary>
public sealed class Wave
{
    /// <summary>The wave name.</summary>
    public string? Name { get; init; }

    /// <summary>The wave description.</summary>
    public string? Description { get; init; }

    /// <summary>The seeds the wave is built from, for example <c>album:12345</c>.</summary>
    public IReadOnlyList<string>? Seeds { get; init; }
}

/// <summary>The visual agent (mascot) presenting a <see cref="Wave"/>.</summary>
public sealed class WaveAgent
{
    /// <summary>The animation URI of the agent.</summary>
    [JsonPropertyName("animation_uri")]
    public string? AnimationUri { get; init; }

    /// <summary>The agent cover art.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The entity the agent represents.</summary>
    public WaveAgentEntity? Entity { get; init; }
}

/// <summary>The entity a <see cref="WaveAgent"/> represents.</summary>
public sealed class WaveAgentEntity
{
    /// <summary>The entity type. Known values: <c>album</c>, <c>artist</c>.</summary>
    public string? Type { get; init; }
}
