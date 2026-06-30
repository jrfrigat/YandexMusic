using System.Text.Json;
using System.Text.Json.Serialization;
using YandexMusic.Models;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads a list of labels that the API may send either as full objects (most endpoints) or as bare
/// strings (label names, as returned in search results). A string element becomes a
/// <see cref="Label"/> with only its <see cref="Label.Name"/> set.
/// </summary>
internal sealed class LabelListConverter : JsonConverter<IReadOnlyList<Label>>
{
    /// <inheritdoc />
    public override IReadOnlyList<Label>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected an array of labels but found {reader.TokenType}.");
        }

        var labels = new List<Label>();
        var labelInfo = YandexMusicJson.TypeInfo<Label>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    labels.Add(new Label { Name = reader.GetString() ?? string.Empty });
                    break;
                case JsonTokenType.StartObject:
                    var label = JsonSerializer.Deserialize(ref reader, labelInfo);
                    if (label is not null)
                    {
                        labels.Add(label);
                    }

                    break;
                default:
                    throw new JsonException($"Cannot convert a JSON {reader.TokenType} token to a label.");
            }
        }

        return labels;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IReadOnlyList<Label> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        var labelInfo = YandexMusicJson.TypeInfo<Label>();
        foreach (var label in value)
        {
            JsonSerializer.Serialize(writer, label, labelInfo);
        }

        writer.WriteEndArray();
    }
}
