using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads a JSON value that the API may send either as a number or as a string into a nullable
/// <see cref="long"/> (for example <c>exec-duration-millis</c>, which appears as <c>166</c> in one
/// response and <c>"166"</c> in another). Unparseable or null values become <see langword="null"/>.
/// </summary>
internal sealed class FlexibleInt64Converter : JsonConverter<long?>
{
    /// <inheritdoc />
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var number) ? number : (long)reader.GetDouble();
            case JsonTokenType.String:
                var text = reader.GetString();
                return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : null;
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Cannot convert a JSON {reader.TokenType} token to a 64-bit integer.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value is { } number)
        {
            writer.WriteNumberValue(number);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
