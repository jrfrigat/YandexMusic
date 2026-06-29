using System.Net;

namespace YandexMusic;

/// <summary>
/// Thrown when the Yandex Music API responds with a non-success HTTP status code or a structured
/// error in the response envelope.
/// </summary>
public sealed class YandexMusicApiException : YandexMusicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicApiException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="error">The structured error parsed from the response, if any.</param>
    /// <param name="rawResponse">The raw response body, preserved for diagnostics.</param>
    public YandexMusicApiException(HttpStatusCode statusCode, ApiError? error, string? rawResponse)
        : base(BuildMessage(statusCode, error))
    {
        StatusCode = statusCode;
        Error = error;
        RawResponse = rawResponse;
    }

    /// <summary>The HTTP status code returned by the API.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>The structured error parsed from the response envelope, or <see langword="null"/>.</summary>
    public ApiError? Error { get; }

    /// <summary>The short error code (<see cref="ApiError.Name"/>), or <see langword="null"/>.</summary>
    public string? ErrorName => Error?.Name;

    /// <summary>The raw response body as returned by the server, preserved for diagnostics.</summary>
    public string? RawResponse { get; }

    private static string BuildMessage(HttpStatusCode statusCode, ApiError? error)
    {
        var code = (int)statusCode;
        return error is null
            ? $"The Yandex Music API request failed with status {code} ({statusCode})."
            : $"The Yandex Music API request failed with status {code} ({statusCode}): {error.Name} {error.Message}".TrimEnd();
    }
}
