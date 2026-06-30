using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads a JSON value that the API may send as an integer (<c>0</c>), a floating-point number
/// (<c>0.0</c>, <c>169.00</c>) or a string (<c>"169"</c>) into a <see cref="decimal"/>. The price
/// <c>amount</c>, for instance, comes back as <c>0.0</c> for some clients and <c>0</c> for others, so a
/// plain integer property fails to deserialize. Null or unparseable values become <c>0</c>.
/// </summary>
internal sealed class FlexibleDecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc />
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetDecimal();
            case JsonTokenType.String:
                var text = reader.GetString();
                return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : 0m;
            case JsonTokenType.Null:
                return 0m;
            default:
                throw new JsonException($"Cannot convert a JSON {reader.TokenType} token to a decimal.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
