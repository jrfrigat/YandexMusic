using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;

namespace YandexMusic.Models.Library;

/// <summary>
/// A single liked catalogue entity (album, artist, or playlist) together with when it was liked.
/// Only the property matching the entity kind is populated; the others remain <see langword="null"/>.
/// </summary>
public sealed class Like
{
    /// <summary>The liked entity kind: <c>album</c>, <c>artist</c>, or <c>playlist</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The liked entity identifier, when reported.</summary>
    public string? Id { get; init; }

    /// <summary>When the entity was liked, as an ISO-8601 timestamp, when known.</summary>
    public string? Timestamp { get; init; }

    /// <summary>The liked album, when this entry is an album like.</summary>
    public Album? Album { get; init; }

    /// <summary>The liked artist, when this entry is an artist like.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The liked playlist, when this entry is a playlist like.</summary>
    public Playlist? Playlist { get; init; }

    /// <summary>A short description of the liked entity, when present.</summary>
    public string? ShortDescription { get; init; }

    /// <summary>A description of the liked entity, when present.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the liked entity is a premiere, when reported.</summary>
    public bool? IsPremiere { get; init; }

    /// <summary>Whether the liked entity is shown as a banner, when reported.</summary>
    public bool? IsBanner { get; init; }
}
