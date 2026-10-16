using System.Globalization;
using System.Text.Json;
using YandexMusic.Exceptions;

namespace YandexMusic.Ynison;

/// <summary>
/// Recognizes the server's <c>error</c> frames. Ynison reports a rejected request or a failing
/// session with a frame of the form <c>{ "error": { "message", "grpc_code", "http_code", "details" } }</c>
/// instead of a state; read as a state it would deserialize into an empty one and be mistaken for
/// "nothing is playing".
/// </summary>
internal static class YnisonServerErrors
{
    private const int GrpcPermissionDenied = 7;
    private const int GrpcUnauthenticated = 16;
    private const string DeviceDisplacedCode = "400090001";

    /// <summary>Checks whether a frame is an error frame and, if so, builds the matching exception.</summary>
    /// <param name="frame">The raw frame text.</param>
    /// <param name="error">The exception describing the error, when the frame is one.</param>
    /// <returns><see langword="true"/> when the frame is an error frame.</returns>
    public static bool TryParse(string frame, out YandexMusicYnisonServerException? error)
    {
        error = null;

        // Error frames are rare and state frames are large: skip the parse unless the key is present.
        if (!frame.Contains("\"error\"", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(frame);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("error", out var payload)
                || payload.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            error = Parse(payload);
            return true;
        }
        catch (JsonException)
        {
            // Not valid JSON at all; the caller's own parse reports it as a malformed frame.
            return false;
        }
    }

    /// <summary>Builds the exception for the content of an <c>error</c> member.</summary>
    /// <param name="payload">The <c>error</c> object.</param>
    /// <returns>The exception, not thrown.</returns>
    public static YandexMusicYnisonServerException Parse(JsonElement payload)
    {
        var details = GetObject(payload, "details") ?? GetObject(payload, "extra_headers");

        var message = GetString(payload, "message");
        var grpcCode = GetInt(payload, "grpc_code");
        var httpCode = GetInt(payload, "http_code");
        var errorCode = details is { } d ? GetString(d, "ynison-error-code") : null;

        var kind = YnisonServerErrorKind.Other;
        if (grpcCode is GrpcUnauthenticated or GrpcPermissionDenied || httpCode is 401 or 403)
        {
            kind = YnisonServerErrorKind.Unauthorized;
        }
        else if (errorCode == DeviceDisplacedCode)
        {
            kind = YnisonServerErrorKind.DeviceDisplaced;
        }

        var goAwaySeconds = details is { } g ? GetInt(g, "ynison-go-away-for-seconds") : null;

        return new YandexMusicYnisonServerException(
            string.IsNullOrEmpty(message) ? "Unknown Ynison error." : message,
            kind,
            grpcCode,
            httpCode,
            errorCode,
            details is { } b ? ParseBackoff(GetString(b, "ynison-backoff-millis")) : [],
            goAwaySeconds is > 0 ? TimeSpan.FromSeconds(goAwaySeconds.Value) : null);
    }

    // The ladder arrives as colon-separated milliseconds, for example "0:1000:5000:30000".
    private static List<TimeSpan> ParseBackoff(string? value)
    {
        var ladder = new List<TimeSpan>();
        if (string.IsNullOrEmpty(value))
        {
            return ladder;
        }

        foreach (var part in value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var milliseconds) && milliseconds >= 0)
            {
                ladder.Add(TimeSpan.FromMilliseconds(milliseconds));
            }
        }

        return ladder;
    }

    private static JsonElement? GetObject(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Object && value.EnumerateObject().Any()
            ? value
            : null;

    private static string? GetString(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }

    // Codes arrive as numbers or as numeric strings depending on the server path.
    private static int? GetInt(JsonElement element, string name)
        => int.TryParse(GetString(element, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : null;
}
