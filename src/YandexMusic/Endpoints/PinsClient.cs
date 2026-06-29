using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Pins;
using YandexMusic.Serialization;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IPinsClient"/> implementation.</summary>
internal sealed class PinsClient : IPinsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new pins endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public PinsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<PinsList?> GetAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<PinsList>("/pins", cancellationToken);

    /// <inheritdoc />
    public Task<Pin?> PinAlbumAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return PinAsync("/pin/album", ("id", albumId), cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> UnpinAlbumAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return UnpinAsync("/pin/album", ("id", albumId), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Pin?> PinArtistAsync(string artistId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);
        return PinAsync("/pin/artist", ("id", artistId), cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> UnpinArtistAsync(string artistId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);
        return UnpinAsync("/pin/artist", ("id", artistId), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Pin?> PinPlaylistAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        return PinAsync("/pin/playlist", cancellationToken, ("uid", userId), ("kind", kind));
    }

    /// <inheritdoc />
    public Task<bool> UnpinPlaylistAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        return UnpinAsync("/pin/playlist", cancellationToken, ("uid", userId), ("kind", kind));
    }

    /// <inheritdoc />
    public Task<Pin?> PinWaveAsync(string seeds, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seeds);
        return PinAsync("/pin/wave", ("seeds", seeds), cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> UnpinWaveAsync(string seeds, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seeds);
        return UnpinAsync("/pin/wave", ("seeds", seeds), cancellationToken);
    }

    private Task<Pin?> PinAsync(string path, (string Key, string Value) field, CancellationToken cancellationToken)
        => PinAsync(path, cancellationToken, field);

    private async Task<Pin?> PinAsync(string path, CancellationToken cancellationToken, params (string Key, string Value)[] fields)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, path) { Content = CreateJsonBody(fields) };
        return await SendForResultAsync<Pin>(request, cancellationToken).ConfigureAwait(false);
    }

    private Task<bool> UnpinAsync(string path, (string Key, string Value) field, CancellationToken cancellationToken)
        => UnpinAsync(path, cancellationToken, field);

    private async Task<bool> UnpinAsync(string path, CancellationToken cancellationToken, params (string Key, string Value)[] fields)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, path) { Content = CreateJsonBody(fields) };
        var result = await SendForResultAsync<string>(request, cancellationToken).ConfigureAwait(false);
        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<T?> SendForResultAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _connection
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
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

    private static StringContent CreateJsonBody((string Key, string Value)[] fields)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            foreach (var (key, value) in fields)
            {
                writer.WriteString(key, value);
            }

            writer.WriteEndObject();
        }

        var json = Encoding.UTF8.GetString(buffer.ToArray());
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }
}
