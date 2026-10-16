using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace YandexMusic.Http;

/// <summary>
/// Computes the request signatures required by signed track endpoints (lyrics and file info). Both
/// schemes are fixed, publicly-known parts of the Yandex Music protocol: an HMAC-SHA256 keyed with a
/// constant app key and base64-encoded, over <c>{trackId}{timestamp}</c> for lyrics and over
/// <c>{timestamp}{trackId}{quality}{codecs}{transport}</c> for file info.
/// </summary>
internal static class TrackRequestSigner
{
    // The signing key embedded in the Android application.
    private const string SignKey = "p93jhgh689SBReK6ghtw62";

    /// <summary>A signature: the Unix timestamp it was made at and the computed value.</summary>
    /// <param name="Timestamp">The Unix time, in seconds, the signature was generated.</param>
    /// <param name="Value">The base64-encoded HMAC-SHA256 signature.</param>
    public readonly record struct Signature(long Timestamp, string Value);

    /// <summary>Creates a signature for the given track at the current time.</summary>
    /// <param name="trackId">The track identifier (the numeric part is used).</param>
    /// <returns>The timestamp and signature value to send as query parameters.</returns>
    public static Signature Sign(string trackId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Sign(trackId, timestamp);
    }

    /// <summary>Creates a signature for the given track at a specific timestamp (used by tests).</summary>
    /// <param name="trackId">The track identifier (the numeric part is used).</param>
    /// <param name="timestamp">The Unix time, in seconds, to embed in the signature.</param>
    /// <returns>The timestamp and signature value.</returns>
    public static Signature Sign(string trackId, long timestamp)
    {
        var number = ExtractTrackNumber(trackId);
        var message = number + timestamp.ToString(CultureInfo.InvariantCulture);

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(SignKey), Encoding.UTF8.GetBytes(message));
        return new Signature(timestamp, Convert.ToBase64String(hash));
    }

    /// <summary>
    /// Creates the signature of a file-info request at the current time. Unlike the lyrics
    /// signature it is keyed with the Windows desktop app key, covers every request parameter, and
    /// is only accepted together with the <see cref="DesktopClientHeader"/> value it belongs to.
    /// </summary>
    /// <param name="trackId">The track identifier exactly as it is sent in the request.</param>
    /// <param name="quality">The requested quality.</param>
    /// <param name="codecs">The accepted codecs, in the order they are sent.</param>
    /// <param name="transport">The requested transport.</param>
    /// <returns>The timestamp and signature value to send as query parameters.</returns>
    public static Signature SignFileInfo(string trackId, string quality, IReadOnlyList<string> codecs, string transport)
        => SignFileInfo(trackId, quality, codecs, transport, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    /// <summary>Creates the signature of a file-info request at a specific timestamp (used by tests).</summary>
    /// <param name="trackId">The track identifier exactly as it is sent in the request.</param>
    /// <param name="quality">The requested quality.</param>
    /// <param name="codecs">The accepted codecs, in the order they are sent.</param>
    /// <param name="transport">The requested transport.</param>
    /// <param name="timestamp">The Unix time, in seconds, to embed in the signature.</param>
    /// <returns>The timestamp and signature value.</returns>
    public static Signature SignFileInfo(string trackId, string quality, IReadOnlyList<string> codecs, string transport, long timestamp)
    {
        var message = timestamp.ToString(CultureInfo.InvariantCulture) + trackId + quality + string.Concat(codecs) + transport;
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(DesktopSignKey), Encoding.UTF8.GetBytes(message));

        // The server expects the base64 text without its final character (the padding "=").
        var value = Convert.ToBase64String(hash);
        return new Signature(timestamp, value[..^1]);
    }

    /// <summary>The <c>X-Yandex-Music-Client</c> value the desktop signing key is bound to.</summary>
    public const string DesktopClientHeader = "YandexMusicDesktopAppWindows/5.95.0";

    // The signing key embedded in the Windows desktop application.
    private const string DesktopSignKey = "kzqU4XhfCaY6B6JTHODeq5";

    // A full track identifier may be "trackId:albumId"; the signature uses only the track number.
    private static string ExtractTrackNumber(string trackId)
    {
        var separator = trackId.IndexOf(':');
        return separator >= 0 ? trackId[..separator] : trackId;
    }
}
