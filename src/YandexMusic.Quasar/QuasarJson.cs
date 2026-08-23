using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace YandexMusic.Quasar;

/// <summary>
/// The source-generation context for the Quasar backend. Its JSON mixes conventions — <c>networkInfo</c>
/// is camelCase while <c>external_port</c> inside it is snake_case — so the camelCase policy covers
/// the majority and the snake_case members carry an explicit name.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(QuasarDeviceListResponse))]
[JsonSerializable(typeof(QuasarTokenResponse))]
internal sealed partial class QuasarJsonContext : JsonSerializerContext;

/// <summary>Typed access to the Quasar context's metadata.</summary>
internal static class QuasarJson
{
    /// <summary>Returns the source-generated metadata for a Quasar response type.</summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <returns>The type's <see cref="JsonTypeInfo{T}"/>.</returns>
    public static JsonTypeInfo<T> TypeInfo<T>()
        => (JsonTypeInfo<T>)QuasarJsonContext.Default.Options.GetTypeInfo(typeof(T));
}
