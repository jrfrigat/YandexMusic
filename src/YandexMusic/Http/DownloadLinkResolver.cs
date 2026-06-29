using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace YandexMusic.Http;

/// <summary>
/// Turns a track download-info granule (the small XML document returned by a variant's
/// <c>downloadInfoUrl</c>) into the final, signed media URL. The signature scheme is a fixed,
/// publicly-known part of the Yandex Music protocol: <c>md5(salt + path[1:] + s)</c>.
/// </summary>
internal static class DownloadLinkResolver
{
    private const string SignSalt = "XGRlBW9FXlekgbPrRHuSiA";

    /// <summary>Builds the final media URL from the download-info XML granule.</summary>
    /// <param name="downloadInfoXml">The XML returned by a variant's <c>downloadInfoUrl</c>.</param>
    /// <returns>The signed, directly-fetchable media URL.</returns>
    /// <exception cref="YandexMusicSerializationException">The XML was empty or missing required fields.</exception>
    public static string BuildDirectLink(string downloadInfoXml)
    {
        var root = XDocument.Parse(downloadInfoXml).Root
            ?? throw new YandexMusicSerializationException("The download-info response was empty.", null);

        var host = root.Element("host")?.Value;
        var path = root.Element("path")?.Value;
        var ts = root.Element("ts")?.Value;
        var s = root.Element("s")?.Value;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ts) || string.IsNullOrEmpty(s))
        {
            throw new YandexMusicSerializationException("The download-info response was missing required fields.", null);
        }

        return $"https://{host}/get-mp3/{Sign(path, s)}/{ts}{path}";
    }

    /// <summary>Computes the lowercase hex MD5 signature for a path and salt token.</summary>
    /// <param name="path">The media path (with its leading slash).</param>
    /// <param name="s">The per-request salt token from the granule.</param>
    /// <returns>The 32-character lowercase hex signature.</returns>
    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms",
        Justification = "MD5 is mandated by the Yandex Music download-URL signing protocol; it is a protocol checksum, not a security primitive.")]
    internal static string Sign(string path, string s)
    {
        var data = Encoding.UTF8.GetBytes(SignSalt + path[1..] + s);
        return Convert.ToHexString(MD5.HashData(data)).ToLowerInvariant();
    }
}
