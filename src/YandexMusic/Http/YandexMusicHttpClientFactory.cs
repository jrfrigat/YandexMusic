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

        var httpClient = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = options.Timeout,
        };

        ConfigureDefaultHeaders(httpClient, options);
        return httpClient;
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
