using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YandexMusic;
using YandexMusic.Authentication;
using YandexMusic.Http;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods that register the YandexMusic client with an
/// <see cref="IServiceCollection"/>.
/// </summary>
public static class YandexMusicServiceCollectionExtensions
{
    private const string HttpClientName = "YandexMusic";

    /// <summary>
    /// Registers <see cref="IYandexMusicClient"/> as a <b>scoped</b> service over a pooled
    /// <see cref="SocketsHttpHandler"/> managed by <see cref="IHttpClientFactory"/>. Each scope (for
    /// example an HTTP request or a signed-in user) receives its own client with an isolated
    /// <see cref="AuthSession"/>, so access tokens never leak between users, while the underlying
    /// connection pool is shared and long-lived. The client is disposed automatically when the scope
    /// ends; the pooled handler is not.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <param name="configure">An optional callback to configure the client options.</param>
    /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddYandexMusic(this IServiceCollection services, Action<YandexMusicClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new YandexMusicClientOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);

        services.AddHttpClient(HttpClientName)
            .ConfigureHttpClient(client =>
            {
                client.Timeout = options.Timeout;
                YandexMusicHttpClientFactory.ConfigureDefaultHeaders(client, options);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // The pooled handler is shared across scopes, so it must not carry a cookie container:
                // API requests authenticate with the per-scope OAuth token header, and the interactive
                // cookie/QR sign-in flows use their own short-lived clients. This keeps users isolated.
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                Proxy = options.Proxy,
                UseProxy = options.Proxy is not null,
            });

        services.TryAddScoped<IYandexMusicClient>(static provider =>
        {
            var options = provider.GetRequiredService<YandexMusicClientOptions>();
            var session = new AuthSession(options.DeviceId);
            var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName);
            return new YandexMusicClient(httpClient, session);
        });

        return services;
    }
}
