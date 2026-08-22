using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace YandexMusic.Quasar.Control;

/// <summary>One message to a speaker. Every field is required; the device ignores nothing.</summary>
/// <param name="ConversationToken">The per-device token from the Quasar backend.</param>
/// <param name="Id">An identifier the caller invents; the answer returns it as <c>requestId</c>.</param>
/// <param name="SentTime">When the message was sent, as a Unix time in milliseconds.</param>
/// <param name="Payload">What the device is being asked to do.</param>
internal sealed record GlagolRequest(
    string ConversationToken,
    string Id,
    long SentTime,
    GlagolPayload Payload);

/// <summary>
/// A command and its argument. The arguments are nullable and omitted when unset, because a device
/// is given exactly the fields its command takes and nothing else.
/// </summary>
/// <param name="Command">The command name, for example <c>play</c> or <c>setVolume</c>.</param>
internal sealed record GlagolPayload(string Command)
{
    /// <summary>The volume for <c>setVolume</c>, from 0.0 to 1.0.</summary>
    public double? Volume { get; init; }
}

/// <summary>
/// The source-generation context for the local control protocol. Both directions are camelCase;
/// the one snake_case member of a reply carries an explicit name on its property.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(GlagolRequest))]
[JsonSerializable(typeof(LocalDeviceFrame))]
internal sealed partial class GlagolJsonContext : JsonSerializerContext;

/// <summary>Typed access to the local control context's metadata.</summary>
internal static class GlagolJson
{
    /// <summary>Returns the source-generated metadata for a protocol type.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <returns>The type's <see cref="JsonTypeInfo{T}"/>.</returns>
    public static JsonTypeInfo<T> TypeInfo<T>()
        => (JsonTypeInfo<T>)GlagolJsonContext.Default.Options.GetTypeInfo(typeof(T));
}
