namespace YandexMusic.Http;

/// <summary>
/// The low-level request engine shared by all endpoint groups. It builds requests against the
/// Yandex Music API, applies authentication, sends them, unwraps the response envelope and maps
/// failures to typed exceptions. A single connection is reused for the lifetime of the client.
/// </summary>
internal interface IYandexMusicConnection
{
    /// <summary>Sends a <c>GET</c> request and returns the unwrapped <c>result</c> payload.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="relativeUrl">A path relative to the API host, or an absolute URL.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The deserialized result, or <see langword="null"/> when the API returns no result.</returns>
    Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken);

    /// <summary>Sends a <c>GET</c> request with extra request headers and returns the unwrapped payload.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="relativeUrl">A path relative to the API host, or an absolute URL.</param>
    /// <param name="headers">Additional request headers, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The deserialized result, or <see langword="null"/> when the API returns no result.</returns>
    Task<T?> GetAsync<T>(string relativeUrl, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken);

    /// <summary>Sends a <c>POST</c> request and returns the unwrapped <c>result</c> payload.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="relativeUrl">A path relative to the API host, or an absolute URL.</param>
    /// <param name="content">The request body, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The deserialized result, or <see langword="null"/> when the API returns no result.</returns>
    Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, CancellationToken cancellationToken);

    /// <summary>Sends a <c>POST</c> request with extra request headers and returns the unwrapped payload.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="relativeUrl">A path relative to the API host, or an absolute URL.</param>
    /// <param name="content">The request body, or <see langword="null"/>.</param>
    /// <param name="headers">Additional request headers, or <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The deserialized result, or <see langword="null"/> when the API returns no result.</returns>
    Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken);

    /// <summary>Sends a raw request and returns the response without unwrapping or disposing it.</summary>
    /// <param name="request">The request to send. Authentication headers are applied automatically.</param>
    /// <param name="completionOption">When the operation should complete.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The HTTP response message.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);
}
