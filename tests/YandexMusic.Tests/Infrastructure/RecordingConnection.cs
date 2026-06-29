using YandexMusic.Http;

namespace YandexMusic.Tests.Infrastructure;

/// <summary>
/// A test double for <see cref="IYandexMusicConnection"/> that records the request an endpoint
/// builds (method, URL, headers and materialized form body) and returns a caller-supplied result.
/// Used to assert the exact wire conventions of each endpoint without a network call.
/// </summary>
internal sealed class RecordingConnection : IYandexMusicConnection
{
    /// <summary>The HTTP method of the most recent request.</summary>
    public HttpMethod? Method { get; private set; }

    /// <summary>The relative URL (path and query) of the most recent request.</summary>
    public string? Url { get; private set; }

    /// <summary>The extra headers attached to the most recent request, if any.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; private set; }

    /// <summary>The materialized form/string body of the most recent request, if any.</summary>
    public string? Body { get; private set; }

    /// <summary>The value returned (cast to the requested type) from the next request.</summary>
    public object? NextResult { get; set; }

    public Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        => GetAsync<T>(relativeUrl, null, cancellationToken);

    public Task<T?> GetAsync<T>(string relativeUrl, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        Record(HttpMethod.Get, relativeUrl, headers, content: null);
        return Task.FromResult((T?)NextResult);
    }

    public Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, CancellationToken cancellationToken)
        => PostAsync<T>(relativeUrl, content, null, cancellationToken);

    public Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        Record(HttpMethod.Post, relativeUrl, headers, content);
        return Task.FromResult((T?)NextResult);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        => throw new NotSupportedException("RecordingConnection does not support raw SendAsync.");

    private void Record(HttpMethod method, string relativeUrl, IReadOnlyDictionary<string, string>? headers, HttpContent? content)
    {
        Method = method;
        Url = relativeUrl;
        Headers = headers;
        Body = content?.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
