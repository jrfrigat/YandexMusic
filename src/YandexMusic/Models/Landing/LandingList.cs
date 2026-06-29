namespace YandexMusic.Models.Landing;

/// <summary>
/// A landing list payload returned by the new-releases, new-playlists, and podcasts endpoints.
/// Only the field matching the list <see cref="Type"/> is populated.
/// </summary>
public sealed class LandingList
{
    /// <summary>The list type, one of <c>new-releases</c>, <c>new-playlists</c>, or <c>podcasts</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The type used when building "from" attribution strings.</summary>
    public string TypeForFrom { get; init; } = string.Empty;

    /// <summary>The list title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The list identifier, when present.</summary>
    public string? Id { get; init; }

    /// <summary>The album ids for a <c>new-releases</c> list.</summary>
    public IReadOnlyList<long> NewReleases { get; init; } = [];

    /// <summary>The playlist references for a <c>new-playlists</c> list.</summary>
    public IReadOnlyList<PlaylistId> NewPlaylists { get; init; } = [];

    /// <summary>The album ids for a <c>podcasts</c> list.</summary>
    public IReadOnlyList<long> Podcasts { get; init; } = [];
}

/// <summary>A reference to a playlist by its owner uid and kind.</summary>
public sealed class PlaylistId
{
    /// <summary>The owner's user identifier.</summary>
    public long Uid { get; init; }

    /// <summary>The playlist kind (its per-owner identifier).</summary>
    public long Kind { get; init; }
}

/// <summary>The result of resolving a tag to its playlist references.</summary>
public sealed class TagResult
{
    /// <summary>The resolved tag, when present.</summary>
    public Tag? Tag { get; init; }

    /// <summary>The playlist references carrying the tag.</summary>
    public IReadOnlyList<PlaylistId> Ids { get; init; } = [];
}

/// <summary>A playlist tag (a curated grouping such as a mood or a genre).</summary>
public sealed class Tag
{
    /// <summary>The tag identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The lower-case tag value.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>The display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The Open Graph description.</summary>
    public string OgDescription { get; init; } = string.Empty;

    /// <summary>The Open Graph image URI, when present.</summary>
    public string? OgImage { get; init; }
}
