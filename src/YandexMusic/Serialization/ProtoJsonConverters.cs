using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YandexMusic.Serialization;

/// <summary>
/// An enum converter for protobuf-JSON messages (Ynison). Writes the canonical proto member name in
/// UPPER_SNAKE_CASE (<c>DO_NOT_INTERCEPT_BY_DEFAULT</c>) and reads tolerantly: the member name in any
/// casing with <c>-</c>/<c>_</c> separators ignored, or the numeric value. Unknown values read back as
/// the enum's <c>0</c> member, mirroring <see cref="TolerantEnumConverter{TEnum}"/>. Referenced as a
/// closed generic so it stays trim/AOT-safe.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
internal sealed class ProtoEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly (string Normalized, TEnum Value)[] Members = BuildMembers();

    /// <inheritdoc />
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        var text = reader.GetString();
        if (string.IsNullOrEmpty(text))
        {
            return default;
        }

        var normalized = Normalize(text);
        foreach (var (memberNormalized, value) in Members)
        {
            if (string.Equals(memberNormalized, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return default;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(ToProtoName(value.ToString()));

    private static (string, TEnum)[] BuildMembers()
    {
        var names = Enum.GetNames<TEnum>();
        var values = Enum.GetValues<TEnum>();
        var members = new (string, TEnum)[names.Length];
        for (var i = 0; i < names.Length; i++)
        {
            members[i] = (Normalize(names[i]), values[i]);
        }

        return members;
    }

    private static string Normalize(string value) => value.Replace("-", string.Empty).Replace("_", string.Empty);

    private static string ToProtoName(string member)
    {
        var builder = new StringBuilder(member.Length + 8);
        foreach (var symbol in member)
        {
            if (char.IsUpper(symbol) && builder.Length > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToUpperInvariant(symbol));
        }

        return builder.ToString();
    }
}

/// <summary>
/// A 64-bit integer converter for protobuf-JSON messages (Ynison). Canonical protobuf-JSON writes
/// int64 as a string; this converter reads both a string and a number and writes a plain number,
/// which every protobuf-JSON parser accepts.
/// </summary>
internal sealed class ProtoInt64Converter : JsonConverter<long>
{
    /// <inheritdoc />
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.Number => reader.GetInt64(),
        JsonTokenType.String when long.TryParse(reader.GetString(), out var value) => value,
        _ => throw new JsonException($"Cannot read a 64-bit integer from a {reader.TokenType} token."),
    };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
