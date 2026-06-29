using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace YandexMusic.Http;

/// <summary>
/// Computes the request signature required by signed track endpoints (currently lyrics). The scheme
/// is a fixed, publicly-known part of the Yandex Music protocol: an HMAC-SHA256 over
/// <c>{trackId}{timestamp}</c> keyed with a constant Android-app key, base64-encoded.
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

    // A full track identifier may be "trackId:albumId"; the signature uses only the track number.
    private static string ExtractTrackNumber(string trackId)
    {
        var separator = trackId.IndexOf(':');
        return separator >= 0 ? trackId[..separator] : trackId;
    }
}
