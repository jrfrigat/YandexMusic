using System.Text.Json;
using YandexMusic.Authentication;
using YandexMusic.Serialization;

namespace YandexMusic.Http;

/// <summary>
/// The default <see cref="IYandexMusicConnection"/> implementation over a single
/// <see cref="HttpClient"/> and the client's <see cref="IAuthSession"/>.
/// </summary>
internal sealed class YandexMusicConnection : IYandexMusicConnection
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;

    /// <summary>Initializes a new connection.</summary>
    /// <param name="httpClient">The HTTP client used to send requests.</param>
    /// <param name="authSession">The session that supplies the access token and device identity.</param>
    public YandexMusicConnection(HttpClient httpClient, IAuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        => SendForResultAsync<T>(HttpMethod.Get, relativeUrl, content: null, headers: null, cancellationToken);

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string relativeUrl, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
        => SendForResultAsync<T>(HttpMethod.Get, relativeUrl, content: null, headers, cancellationToken);

    /// <inheritdoc />
    public Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, CancellationToken cancellationToken)
        => SendForResultAsync<T>(HttpMethod.Post, relativeUrl, content, headers: null, cancellationToken);

    /// <inheritdoc />
    public Task<T?> PostAsync<T>(string relativeUrl, HttpContent? content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
        => SendForResultAsync<T>(HttpMethod.Post, relativeUrl, content, headers, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        ApplyAuthentication(request);
        return _httpClient.SendAsync(request, completionOption, cancellationToken);
    }

    private async Task<T?> SendForResultAsync<T>(HttpMethod method, string relativeUrl, HttpContent? content, IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, relativeUrl, content, headers);
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            if (!response.IsSuccessStatusCode)
            {
                throw await CreateApiExceptionAsync(response, stream, cancellationToken).ConfigureAwait(false);
            }

            try
            {
                var envelope = await JsonSerializer
                    .DeserializeAsync(stream, YandexMusicJson.TypeInfo<ApiResponse<T>>(), cancellationToken)
                    .ConfigureAwait(false);

                if (envelope?.Error is { } error)
                {
                    throw new YandexMusicApiException(response.StatusCode, error, rawResponse: null);
                }

                return envelope is null ? default : envelope.Result;
            }
            catch (JsonException ex)
            {
                throw new YandexMusicSerializationException("Failed to deserialize the Yandex Music API response.", ex);
            }
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, HttpContent? content, IReadOnlyDictionary<string, string>? headers)
    {
        // Resolve relative paths against the API host. An already-absolute URL (for example a signed
        // media URL) is used as-is; building via Uri parsing avoids the fragile StartsWith("http") heuristic.
        var uri = Uri.TryCreate(relativeUrl, UriKind.Absolute, out var absolute)
            ? absolute
            : new Uri(new Uri(YandexMusicHosts.Api), relativeUrl);

        var request = new HttpRequestMessage(method, uri) { Content = content };
        ApplyAuthentication(request);

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return request;
    }

    private void ApplyAuthentication(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_authSession.AccessToken))
        {
            request.Headers.TryAddWithoutValidation(YandexMusicHeaders.Authorization, "OAuth " + _authSession.AccessToken);
        }

        request.Headers.TryAddWithoutValidation(YandexMusicHeaders.Client, _authSession.DeviceId);
    }

    private static async Task<YandexMusicApiException> CreateApiExceptionAsync(HttpResponseMessage response, Stream stream, CancellationToken cancellationToken)
    {
        string? raw = null;
        try
        {
            // leaveOpen: the single `await using` in SendForResultAsync owns the stream's lifetime.
            using var reader = new StreamReader(stream, leaveOpen: true);
            raw = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // The body could not be read — still surface the status code below.
        }

        ApiError? error = null;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                error = JsonSerializer.Deserialize(raw, YandexMusicJson.TypeInfo<ErrorEnvelope>())?.Error;
            }
            catch (JsonException)
            {
                // The body is not a structured error envelope — keep the raw text only.
            }
        }

        return new YandexMusicApiException(response.StatusCode, error, raw);
    }
}
