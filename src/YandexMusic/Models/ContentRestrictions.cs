namespace YandexMusic.Models;

/// <summary>Availability restrictions and disclaimers attached to a catalogue entity.</summary>
public sealed class ContentRestrictions
{
    /// <summary>Whether the entity is available in the current region, when reported.</summary>
    public bool? Available { get; init; }

    /// <summary>The disclaimer codes that apply to the entity, when any.</summary>
    public IReadOnlyList<string>? Disclaimers { get; init; }
}
