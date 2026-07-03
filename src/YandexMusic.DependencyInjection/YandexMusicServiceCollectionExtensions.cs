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
    /// <summary>
    /// The name of the <see cref="HttpClient"/> that <see cref="AddYandexMusic"/> registers with
    /// <see cref="IHttpClientFactory"/>. Prefer the <c>configureHttpClient</c> callback on
    /// <see cref="AddYandexMusic"/> to customize it; this is exposed for the rare case where a
    /// consumer needs to reach it through a separate <c>services.AddHttpClient(HttpClientName)</c> call.
    /// </summary>
    public const string HttpClientName = "YandexMusic";

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
    /// <param name="configureHttpClient">
    /// An optional callback to further customize the underlying <see cref="IHttpClientBuilder"/> — for
    /// example to attach a resilience handler (<c>AddStandardResilienceHandler</c>), a logging
    /// <see cref="DelegatingHandler"/>, or a custom primary handler.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddYandexMusic(
        this IServiceCollection services,
        Action<YandexMusicClientOptions>? configure = null,
        Action<IHttpClientBuilder>? configureHttpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // A factory registration (rather than a pre-built instance) so TryAddSingleton is the single
        // source of truth: if a consumer already registered YandexMusicClientOptions, this is a no-op
        // and every subsequent resolution (HTTP client config, primary handler, client factory) reads
        // that one instance instead of a locally-captured copy.
        services.TryAddSingleton(_ =>
        {
            var options = new YandexMusicClientOptions();
            configure?.Invoke(options);
            return options;
        });

        var builder = services.AddHttpClient(HttpClientName)
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<YandexMusicClientOptions>();
                client.BaseAddress = options.ApiBaseUri;
                client.Timeout = options.Timeout;
                YandexMusicHttpClientFactory.ConfigureDefaultHeaders(client, options);
                YandexMusicHttpClientFactory.ConfigureProtocolVersion(client);
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var options = provider.GetRequiredService<YandexMusicClientOptions>();
                return new SocketsHttpHandler
                {
                    // The pooled handler is shared across scopes, so it must not carry a cookie container:
                    // API requests authenticate with the per-scope OAuth token header, and the interactive
                    // cookie/QR sign-in flows use their own short-lived clients. This keeps users isolated.
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.All,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                    Proxy = options.Proxy,
                    UseProxy = options.Proxy is not null,
                };
            });

        configureHttpClient?.Invoke(builder);

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
