using YandexMusic.Models.Artists;

namespace YandexMusic.Models;

/// <summary>A short music video clip associated with one or more tracks.</summary>
public sealed class Clip
{
    /// <summary>The clip identifier.</summary>
    public int? ClipId { get; init; }

    /// <summary>The clip title.</summary>
    public string? Title { get; init; }

    /// <summary>A qualifier shown next to the title, when present.</summary>
    public string? Version { get; init; }

    /// <summary>The identifier of the external player used to play the clip.</summary>
    public string? PlayerId { get; init; }

    /// <summary>The clip's universally unique identifier.</summary>
    public string? Uuid { get; init; }

    /// <summary>The thumbnail image URI template, when available.</summary>
    public string? Thumbnail { get; init; }

    /// <summary>The preview video URL, when available.</summary>
    public string? PreviewUrl { get; init; }

    /// <summary>The clip duration, in seconds.</summary>
    public int? Duration { get; init; }

    /// <summary>The identifiers of the tracks the clip is associated with.</summary>
    public IReadOnlyList<int>? TrackIds { get; init; }

    /// <summary>The artists credited on the clip.</summary>
    public IReadOnlyList<Artist>? Artists { get; init; }

    /// <summary>The disclaimer codes that apply to the clip.</summary>
    public IReadOnlyList<string>? Disclaimers { get; init; }

    /// <summary>Whether the clip is marked explicit.</summary>
    public bool? Explicit { get; init; }

    /// <summary>The clip cover art, when available.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The availability restrictions for the clip, when present.</summary>
    public ContentRestrictions? ContentRestrictions { get; init; }
}
