using System.Text.Json.Serialization;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Albums;

/// <summary>The kind of content an <see cref="Album"/> holds.</summary>
[JsonConverter(typeof(TolerantEnumConverter<AlbumMetaType>))]
public enum AlbumMetaType
{
    /// <summary>An unrecognised album type.</summary>
    Unknown = 0,

    /// <summary>A music album.</summary>
    Music,

    /// <summary>A podcast.</summary>
    Podcast,

    /// <summary>An audiobook.</summary>
    AudioBook,
}

/// <summary>The position of a track within an album (disc volume and one-based index).</summary>
public sealed class TrackPosition
{
    /// <summary>The disc/volume number, starting at 1.</summary>
    public int Volume { get; init; }

    /// <summary>The track index within the volume, starting at 1.</summary>
    public int Index { get; init; }
}

/// <summary>
/// An album. This is the shape returned inline inside tracks; the album endpoints return the same
/// type with additional fields populated.
/// </summary>
public sealed class Album
{
    /// <summary>The album identifier.</summary>
    public long Id { get; init; }

    /// <summary>The album title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The kind of content the album holds.</summary>
    public AlbumMetaType MetaType { get; init; }

    /// <summary>The album classification (for example <c>single</c> or <c>compilation</c>), when present.</summary>
    public string? Type { get; init; }

    /// <summary>A content warning such as <c>explicit</c>, when present.</summary>
    public string? ContentWarning { get; init; }

    /// <summary>The release year, when known.</summary>
    public int? Year { get; init; }

    /// <summary>The full release date, when known.</summary>
    public DateTimeOffset? ReleaseDate { get; init; }

    /// <summary>The cover URI template (with a <c>%%</c> size placeholder), when available.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The structured cover image, when available.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The Open Graph image URI template, when available.</summary>
    public string? OgImage { get; init; }

    /// <summary>The accent colours derived from the cover art, when available.</summary>
    public DerivedColors? DerivedColors { get; init; }

    /// <summary>The primary genre identifier, when known.</summary>
    public string? Genre { get; init; }

    /// <summary>The meta-tag identifier the album belongs to, when known.</summary>
    public string? MetaTagId { get; init; }

    /// <summary>The record labels the album was released under.</summary>
    public IReadOnlyList<Label>? Labels { get; init; }

    /// <summary>Whether the album has a trailer.</summary>
    public bool HasTrailer { get; init; }

    /// <summary>The number of tracks in the album.</summary>
    public int TrackCount { get; init; }

    /// <summary>The number of users who liked the album.</summary>
    public int LikesCount { get; init; }

    /// <summary>The artists credited on the album.</summary>
    public IReadOnlyList<Artist> Artists { get; init; } = [];

    /// <summary>Whether the album is available in the current region.</summary>
    public bool Available { get; init; }

    /// <summary>Whether the album is available to Yandex Plus subscribers.</summary>
    public bool AvailableForPremiumUsers { get; init; }

    /// <summary>Whether the album is available on mobile.</summary>
    public bool AvailableForMobile { get; init; }

    /// <summary>Whether only part of the album is available.</summary>
    public bool AvailablePartially { get; init; }

    /// <summary>The identifiers of the album's "best" tracks, when provided.</summary>
    public IReadOnlyList<long>? Bests { get; init; }

    /// <summary>The position of the containing track within this album, when present.</summary>
    public TrackPosition? TrackPosition { get; init; }

    /// <summary>
    /// The album's tracks, grouped by disc/volume. Only populated by the "with tracks" endpoint;
    /// <see langword="null"/> otherwise.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<Track>>? Volumes { get; init; }

    /// <summary>The order tracks are sorted in, when provided by the "with tracks" endpoint.</summary>
    public string? SortOrder { get; init; }

    /// <summary>Pagination information, when provided by the "with tracks" endpoint.</summary>
    public Pager? Pager { get; init; }
}
