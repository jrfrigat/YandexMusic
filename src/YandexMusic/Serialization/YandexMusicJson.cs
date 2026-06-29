using System.Text.Json.Serialization.Metadata;
using YandexMusic.Http;

namespace YandexMusic.Serialization;

/// <summary>
/// Access point for the source-generated JSON metadata. All serialization goes through the
/// <see cref="YandexMusicJsonContext"/>, whose compile-time options (camelCase, case-insensitive)
/// and per-type metadata keep the hot path allocation-friendly and trim/AOT-safe. Enum conversion is
/// attribute-driven (see <see cref="TolerantEnumConverter{TEnum}"/>), so no runtime converters are
/// added.
/// </summary>
internal static class YandexMusicJson
{
    /// <summary>Returns the source-generated metadata for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to resolve metadata for. Must be registered in the context.</typeparam>
    /// <returns>The strongly-typed <see cref="JsonTypeInfo{T}"/> for <typeparamref name="T"/>.</returns>
    public static JsonTypeInfo<T> TypeInfo<T>()
        => (JsonTypeInfo<T>)YandexMusicJsonContext.Default.Options.GetTypeInfo(typeof(T));

    /// <summary>Returns the metadata for the response envelope wrapping <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <returns>The metadata for <see cref="ApiResponse{T}"/>.</returns>
    public static JsonTypeInfo<ApiResponse<T>> ResponseInfo<T>() => TypeInfo<ApiResponse<T>>();
}
