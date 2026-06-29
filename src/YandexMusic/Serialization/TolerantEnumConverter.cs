using System.Text.Json;
using System.Text.Json.Serialization;

namespace YandexMusic.Serialization;

/// <summary>
/// A tolerant string-to-enum converter. It matches the JSON string against the enum member names
/// case-insensitively and ignoring <c>-</c> and <c>_</c> separators (so <c>"from-album-cover"</c>
/// and <c>"COVER_ONLY"</c> map to <c>FromAlbumCover</c> and <c>CoverOnly</c>), and returns the
/// enum's <c>0</c> value for any unrecognised input. This keeps deserialization robust when the API
/// introduces new values. Designed to be referenced as a closed generic
/// (<c>[JsonConverter(typeof(TolerantEnumConverter&lt;MyEnum&gt;))]</c>) so it stays trim/AOT-safe.
/// </summary>
/// <typeparam name="TEnum">The enum type to convert.</typeparam>
internal sealed class TolerantEnumConverter<TEnum> : JsonConverter<TEnum>
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
        => writer.WriteStringValue(value.ToString());

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
}
