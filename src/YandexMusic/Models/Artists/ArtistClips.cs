namespace YandexMusic.Models.Artists;

/// <summary>The result of the artist "artist-clips" block: a page of an artist's clips.</summary>
public sealed class ArtistClips
{
    /// <summary>The clip items on this page.</summary>
    public IReadOnlyList<ArtistClipItem>? Items { get; init; }

    /// <summary>Pagination information, when present.</summary>
    public Pager? Pager { get; init; }
}

/// <summary>An entry within an <see cref="ArtistClips"/> result.</summary>
public sealed class ArtistClipItem
{
    /// <summary>The item type (for example <c>clip</c>).</summary>
    public string? Type { get; init; }

    /// <summary>The item payload, when present.</summary>
    public ArtistClipData? Data { get; init; }
}

/// <summary>The payload of an <see cref="ArtistClipItem"/>.</summary>
public sealed class ArtistClipData
{
    /// <summary>The clip, when present.</summary>
    public Clip? Clip { get; init; }

    /// <summary>The artists credited on the clip, when present.</summary>
    public IReadOnlyList<Artist>? Artists { get; init; }
}
