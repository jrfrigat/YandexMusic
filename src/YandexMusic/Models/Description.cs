namespace YandexMusic.Models;

/// <summary>A rich text description with an optional source link (for example an artist biography).</summary>
public sealed class Description
{
    /// <summary>The description text.</summary>
    public string? Text { get; init; }

    /// <summary>The source URL of the description, when provided.</summary>
    public string? Uri { get; init; }
}
