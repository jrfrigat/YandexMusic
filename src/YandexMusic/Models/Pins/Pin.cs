using System.Text.Json.Serialization;

namespace YandexMusic.Models.Pins;

/// <summary>The collection of items a user has pinned to the top of their library.</summary>
public sealed class PinsList
{
    /// <summary>The pinned entries, in display order, when any are present.</summary>
    public IReadOnlyList<Pin>? Pins { get; init; }
}

/// <summary>A single pinned entry referencing an artist, album, playlist or wave.</summary>
public sealed class Pin
{
    /// <summary>
    /// The entry kind. Known values include <c>artist_item</c>, <c>album_item</c>,
    /// <c>playlist_item</c> and <c>wave_item</c>.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The pinned entity payload, when provided.</summary>
    public PinData? Data { get; init; }
}

/// <summary>The entity payload carried by a <see cref="Pin"/>, shaped by its <see cref="Pin.Type"/>.</summary>
public sealed class PinData
{
    /// <summary>The artist or album identifier, for entity-backed pins.</summary>
    public long? Id { get; init; }

    /// <summary>The playlist owner identifier, for playlist pins.</summary>
    public long? Uid { get; init; }

    /// <summary>The playlist number within its owner's library, for playlist pins.</summary>
    public long? Kind { get; init; }

    /// <summary>The playlist universal identifier, for playlist pins.</summary>
    [JsonPropertyName("playlist_uuid")]
    public string? PlaylistUuid { get; init; }

    /// <summary>The artist name, for artist pins.</summary>
    public string? Name { get; init; }

    /// <summary>The album or playlist title.</summary>
    public string? Title { get; init; }

    /// <summary>The entity cover image, when provided.</summary>
    public Cover? Cover { get; init; }

    /// <summary>Availability restrictions and disclaimers attached to the entity, when reported.</summary>
    [JsonPropertyName("content_restrictions")]
    public ContentRestrictions? ContentRestrictions { get; init; }
}
