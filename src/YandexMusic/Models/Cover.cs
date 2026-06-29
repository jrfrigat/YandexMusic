using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models;

/// <summary>How a <see cref="Cover"/> image is sourced.</summary>
[JsonConverter(typeof(TolerantEnumConverter<CoverType>))]
public enum CoverType
{
    /// <summary>An unrecognised cover type.</summary>
    Unknown = 0,

    /// <summary>The cover is taken from the album artwork.</summary>
    FromAlbumCover,

    /// <summary>A single picture cover.</summary>
    Pic,

    /// <summary>A mosaic assembled from several images (see <see cref="Cover.ItemsUri"/>).</summary>
    Mosaic,
}

/// <summary>
/// A cover image reference. Yandex returns URI templates that contain a <c>%%</c> placeholder which
/// must be replaced with a size such as <c>200x200</c> to obtain a concrete image URL.
/// </summary>
public sealed class Cover
{
    /// <summary>How the cover is sourced.</summary>
    public CoverType Type { get; init; }

    /// <summary>The cover URI template (with a <c>%%</c> size placeholder), when available.</summary>
    public string? Uri { get; init; }

    /// <summary>The internal cover prefix, when available.</summary>
    public string? Prefix { get; init; }

    /// <summary>For <see cref="CoverType.Mosaic"/> covers, the URI templates of the tiles.</summary>
    public IReadOnlyList<string>? ItemsUri { get; init; }
}
