using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads a JSON value that the API may send either as a string or as a number into a
/// <see cref="string"/> (for example a track identifier that appears as <c>4</c> in one response and
/// <c>"4"</c> in another). Writes the value back as a JSON string.
/// </summary>
internal sealed class FlexibleStringConverter : JsonConverter<string>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var value)
                    ? value.ToString(CultureInfo.InvariantCulture)
                    : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Cannot convert a JSON {reader.TokenType} token to a string.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
