using System.Text.Json.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>The answer of the file-info endpoint: where and how to download one rendition of a track.</summary>
public sealed class TrackFileInfo
{
    /// <summary>The rendition the server picked, or <see langword="null"/> when none is available.</summary>
    [JsonPropertyName("downloadInfo")]
    public TrackFileDownloadInfo? DownloadInfo { get; init; }
}

/// <summary>
/// One downloadable rendition of a track, including lossless ones. The server picks it from the
/// requested codecs, preferring the requested quality and falling back to the best one available.
/// </summary>
/// <remarks>
/// With the <c>encraw</c> transport the file is encrypted with <see cref="Key"/> (AES-128 in CTR mode
/// with a zero initial counter); <see cref="Endpoints.ITracksClient.DownloadAsync"/> decrypts it
/// while streaming. The container is never changed: <c>flac-mp4</c> and <c>aac-mp4</c> stay MP4.
/// </remarks>
public sealed class TrackFileDownloadInfo
{
    /// <summary>The track identifier as it was requested.</summary>
    [JsonPropertyName("trackId")]
    public string TrackId { get; init; } = string.Empty;

    /// <summary>The quality of the rendition. Known values: <c>lossless</c>, <c>hq</c>, <c>nq</c>.</summary>
    public string Quality { get; init; } = string.Empty;

    /// <summary>
    /// The codec. Known values: <c>flac</c>, <c>flac-mp4</c>, <c>aac-mp4</c>, <c>he-aac-mp4</c>,
    /// <c>aac</c>, <c>he-aac</c>, <c>mp3</c>.
    /// </summary>
    public string Codec { get; init; } = string.Empty;

    /// <summary>The bitrate in kbit/s. It is <c>0</c> for <c>flac-mp4</c>.</summary>
    public int Bitrate { get; init; }

    /// <summary>How the file is delivered. Known values: <c>raw</c> (plain) and <c>encraw</c> (encrypted).</summary>
    public string Transport { get; init; } = string.Empty;

    /// <summary>The file URL.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Mirror URLs of the same file on other servers.</summary>
    public IReadOnlyList<string> Urls { get; init; } = [];

    /// <summary>The canonical track identifier.</summary>
    [JsonPropertyName("realId")]
    public string? RealId { get; init; }

    /// <summary>Whether gain normalization applies.</summary>
    public bool? Gain { get; init; }

    /// <summary>The hexadecimal decryption key, for the <c>encraw</c> transport.</summary>
    public string? Key { get; init; }

    /// <summary>Whether the file must be decrypted with <see cref="Key"/> after download.</summary>
    [JsonIgnore]
    public bool IsEncrypted
        => string.Equals(Transport, "encraw", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(Key);
}
