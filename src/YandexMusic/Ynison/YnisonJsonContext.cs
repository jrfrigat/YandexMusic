using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace YandexMusic.Ynison;

/// <summary>
/// The source-generation context for the Ynison websocket protocol. Unlike the REST API, Ynison
/// frames carry protobuf field names in their original snake_case (<c>player_state</c>,
/// <c>device_id</c>), so this context applies <see cref="JsonNamingPolicy.SnakeCaseLower"/> instead
/// of the main context's camelCase policy.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PutYnisonStateRequest))]
[JsonSerializable(typeof(PutYnisonStateResponse))]
[JsonSerializable(typeof(RedirectResponse))]
internal sealed partial class YnisonJsonContext : JsonSerializerContext;

/// <summary>Typed access to the context's metadata, mirroring <see cref="Serialization.YandexMusicJson"/>.</summary>
internal static class YnisonJson
{
    /// <summary>Returns the source-generated metadata for a Ynison message type.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The type's <see cref="JsonTypeInfo{T}"/> from <see cref="YnisonJsonContext"/>.</returns>
    public static JsonTypeInfo<T> TypeInfo<T>()
        => (JsonTypeInfo<T>)YnisonJsonContext.Default.Options.GetTypeInfo(typeof(T));
}
