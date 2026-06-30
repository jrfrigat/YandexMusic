using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Http;

/// <summary>
/// The envelope every Yandex Music API endpoint wraps its payload in:
/// <c>{ "invocationInfo": { … }, "result": { … }, "error": { … } }</c>.
/// </summary>
/// <typeparam name="T">The type of the <c>result</c> payload.</typeparam>
internal sealed class ApiResponse<T>
{
    /// <summary>The payload returned on success.</summary>
    public T? Result { get; init; }

    /// <summary>The structured error returned on failure, if any.</summary>
    public ApiError? Error { get; init; }

    /// <summary>Diagnostic information about the API call.</summary>
    public InvocationInfo? InvocationInfo { get; init; }
}

/// <summary>A lightweight envelope used to extract just the <c>error</c> field from a failed response.</summary>
internal sealed class ErrorEnvelope
{
    /// <summary>The structured error returned by the API, if present.</summary>
    public ApiError? Error { get; init; }
}

/// <summary>Diagnostic information the API attaches to every response.</summary>
internal sealed class InvocationInfo
{
    /// <summary>The request identifier assigned by the server.</summary>
    [JsonPropertyName("req-id")]
    public string? ReqId { get; init; }

    /// <summary>The server host that handled the request.</summary>
    public string? Hostname { get; init; }

    /// <summary>The server-side execution duration, in milliseconds.</summary>
    [JsonPropertyName("exec-duration-millis")]
    [JsonConverter(typeof(FlexibleInt64Converter))]
    public long? ExecDurationMillis { get; init; }
}
