namespace YandexMusic.Models.Disclaimers;

/// <summary>A legal disclaimer attached to a catalogue entity.</summary>
public sealed class Disclaimer
{
    /// <summary>The foreign-agent notice, when one applies.</summary>
    public ForeignAgent? ForeignAgent { get; init; }
}

/// <summary>A foreign-agent notice required by local regulation.</summary>
public sealed class ForeignAgent
{
    /// <summary>The reason the notice applies. Known value: <c>policy</c>.</summary>
    public string? Reason { get; init; }

    /// <summary>The warning text to display to the user.</summary>
    public string? Title { get; init; }
}
