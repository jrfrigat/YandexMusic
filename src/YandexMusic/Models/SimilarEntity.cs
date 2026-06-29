namespace YandexMusic.Models;

/// <summary>An item returned by the "similar entities" endpoints (a wave-agent recommendation).</summary>
public sealed class SimilarEntityItem
{
    /// <summary>The item type. Known value: <c>wave_agent_item</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The item payload, when present.</summary>
    public SimilarEntityData? Data { get; init; }
}

/// <summary>The payload of a <see cref="SimilarEntityItem"/>.</summary>
public sealed class SimilarEntityData
{
    /// <summary>The wave the item recommends, when present.</summary>
    public Wave? Wave { get; init; }

    /// <summary>The agent presenting the wave, when present.</summary>
    public WaveAgent? Agent { get; init; }
}
