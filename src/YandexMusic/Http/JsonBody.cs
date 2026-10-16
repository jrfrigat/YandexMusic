using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace YandexMusic.Http;

/// <summary>
/// Builds <c>application/json</c> request bodies for endpoints that the API accepts only as JSON.
/// Several rotor endpoints used to take form-encoded fields and now reject them, and they are also
/// strict about value types: a timestamp, for instance, has to be a JSON string in ISO 8601 form or
/// a JSON number, never a quoted number.
/// </summary>
internal static class JsonBody
{
    /// <summary>Wraps a pre-rendered JSON document in a request body.</summary>
    /// <param name="json">The JSON document.</param>
    /// <returns>The body, with its content type set to <c>application/json</c>.</returns>
    public static StringContent FromJson(string json)
    {
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    /// <summary>Builds a JSON object body by writing it with the supplied callback.</summary>
    /// <param name="write">Writes the object members; the surrounding braces are written here.</param>
    /// <returns>The body, with its content type set to <c>application/json</c>.</returns>
    public static StringContent FromObject(Action<Utf8JsonWriter> write)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            write(writer);
            writer.WriteEndObject();
        }

        return FromJson(Encoding.UTF8.GetString(buffer.ToArray()));
    }

    /// <summary>
    /// Formats an event time the way the rotor endpoints expect it: ISO 8601 in UTC with
    /// milliseconds, for example <c>2024-01-01T12:00:00.000Z</c>. The API rejects a Unix timestamp
    /// sent as a string, so this is the only safe textual form.
    /// </summary>
    /// <param name="timestamp">The event time, or <see langword="null"/> for the current time.</param>
    /// <returns>The formatted timestamp.</returns>
    public static string FormatTimestamp(DateTimeOffset? timestamp)
        => (timestamp ?? DateTimeOffset.UtcNow)
            .ToUniversalTime()
            .ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
}
