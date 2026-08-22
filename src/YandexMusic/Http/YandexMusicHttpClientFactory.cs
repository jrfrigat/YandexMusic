using System.Net;
using System.Net.Http.Headers;
using YandexMusic.Authentication;

namespace YandexMusic.Http;

/// <summary>
/// Builds a pre-configured <see cref="HttpClient"/> for the Yandex Music API: connection pooling,
/// automatic decompression, the session cookie container, an optional proxy and the default
/// headers every request needs.
/// </summary>
internal static class YandexMusicHttpClientFactory
{
    private const string DefaultUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36";

    /// <summary>
    /// Creates an owned <see cref="HttpClient"/> configured from <paramref name="options"/> and bound
    /// to the cookie container of <paramref name="authSession"/>.
    /// </summary>
    /// <param name="options">The client options.</param>
    /// <param name="authSession">The session whose cookie container backs the handler.</param>
    /// <returns>A configured <see cref="HttpClient"/> that owns its handler.</returns>
    public static HttpClient Create(YandexMusicClientOptions options, IAuthSession authSession)
    {
        var handler = new SocketsHttpHandler
        {
            CookieContainer = authSession.Cookies,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            Proxy = options.Proxy,
            UseProxy = options.Proxy is not null,
        };

        // A consumer-supplied handler (logging, tracing, retries) wraps ours, so it observes the
        // request exactly as it goes on the wire and the response exactly as it comes back.
        HttpMessageHandler pipeline = handler;
        if (options.HandlerFactory?.Invoke() is { } outer)
        {
            outer.InnerHandler = handler;
            pipeline = outer;
        }

        var httpClient = new HttpClient(pipeline, disposeHandler: true)
        {
            BaseAddress = options.ApiBaseUri,
            Timeout = options.Timeout,
        };

        ConfigureDefaultHeaders(httpClient, options);
        ConfigureProtocolVersion(httpClient);
        return httpClient;
    }

    /// <summary>
    /// Prefers HTTP/2 (connection multiplexing) while transparently falling back to HTTP/1.1 when the
    /// server or an intermediary does not negotiate it. Applied to every client the library creates.
    /// </summary>
    /// <param name="httpClient">The client to configure.</param>
    public static void ConfigureProtocolVersion(HttpClient httpClient)
    {
        httpClient.DefaultRequestVersion = HttpVersion.Version20;
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
    }

    /// <summary>
    /// Applies the default request headers (<c>User-Agent</c> and <c>Accept-Language</c>) to an
    /// existing client. Used both by <see cref="Create"/> and by the dependency-injection
    /// integration, which supplies its own pooled handler.
    /// </summary>
    /// <param name="httpClient">The client to configure.</param>
    /// <param name="options">The client options.</param>
    public static void ConfigureDefaultHeaders(HttpClient httpClient, YandexMusicClientOptions options)
    {
        var userAgent = string.IsNullOrWhiteSpace(options.UserAgent) ? DefaultUserAgent : options.UserAgent;
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, userAgent);
        httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(options.Language);
    }

    private static class HeaderNames
    {
        public const string UserAgent = "User-Agent";
    }
}
