using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>A radio station descriptor (icons, identity and its tunable restrictions).</summary>
public sealed class Station
{
    /// <summary>The station identity (type and tag).</summary>
    public StationId? Id { get; init; }

    /// <summary>The station display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The primary station icon.</summary>
    public Icon Icon { get; init; } = new();

    /// <summary>The MTS-branded station icon.</summary>
    [JsonPropertyName("mtsIcon")]
    public Icon MtsIcon { get; init; } = new();

    /// <summary>The geocell station icon.</summary>
    [JsonPropertyName("geocellIcon")]
    public Icon GeocellIcon { get; init; } = new();

    /// <summary>The seed value used in the <c>from</c> field when reporting playback.</summary>
    [JsonPropertyName("idForFrom")]
    public string IdForFrom { get; init; } = string.Empty;

    /// <summary>The settings the station accepts, with their allowed values.</summary>
    public Restrictions Restrictions { get; init; } = new();

    /// <summary>The newer restrictions set, with their allowed values.</summary>
    public Restrictions Restrictions2 { get; init; } = new();

    /// <summary>The full-size station image URL, when provided.</summary>
    [JsonPropertyName("fullImageUrl")]
    public string? FullImageUrl { get; init; }

    /// <summary>The MTS-branded full-size station image URL, when provided.</summary>
    [JsonPropertyName("mtsFullImageUrl")]
    public string? MtsFullImageUrl { get; init; }

    /// <summary>The parent station identity, when this station is nested.</summary>
    [JsonPropertyName("parentId")]
    public StationId? ParentId { get; init; }
}

/// <summary>The identity of a radio station: a type and a tag (for example <c>genre:pop</c>).</summary>
public sealed class StationId
{
    /// <summary>The station type, for example <c>genre</c> or <c>user</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The station tag scoped to its type, for example <c>pop</c>.</summary>
    public string Tag { get; init; } = string.Empty;
}

/// <summary>A station icon: a background color and an image template URL.</summary>
public sealed class Icon
{
    /// <summary>The icon background color (for example <c>#FFFFFF</c>).</summary>
    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; init; } = string.Empty;

    /// <summary>The icon image URL template.</summary>
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; init; } = string.Empty;
}
