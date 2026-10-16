using System.Text.Json;
using System.Text.Json.Serialization;
using YandexMusic.Models;
using YandexMusic.Models.Radio;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads an item of a combined radio session: an object of the form
/// <c>{ "type": "TRACK", "data": { ... } }</c> whose <c>data</c> is deserialized into the typed
/// property matching <c>type</c>. An unrecognized type leaves both payload properties empty rather
/// than failing, so a new item kind added by the server does not break the whole batch.
/// </summary>
internal sealed class CombinedSessionItemConverter : JsonConverter<CombinedSessionItem>
{
    /// <inheritdoc />
    public override CombinedSessionItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var type = root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : default;

        Track? track = null;
        Clip? clip = null;

        if (data.ValueKind == JsonValueKind.Object)
        {
            if (string.Equals(type, "TRACK", StringComparison.OrdinalIgnoreCase))
            {
                track = data.Deserialize(YandexMusicJson.TypeInfo<Track>());
            }
            else if (string.Equals(type, "CLIP", StringComparison.OrdinalIgnoreCase))
            {
                clip = data.Deserialize(YandexMusicJson.TypeInfo<Clip>());
            }
        }

        return new CombinedSessionItem { Type = type, Track = track, Clip = clip };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CombinedSessionItem value, JsonSerializerOptions options)
        => throw new NotSupportedException("CombinedSessionItem is a read-only response model.");
}
