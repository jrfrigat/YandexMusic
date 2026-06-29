using YandexMusic.Http;
using YandexMusic.Models.Albums;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IAlbumsClient"/> implementation.</summary>
internal sealed class AlbumsClient : IAlbumsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new albums endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public AlbumsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Album?> GetAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return _connection.GetAsync<Album>($"/albums/{Uri.EscapeDataString(albumId)}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Album>> GetManyAsync(IEnumerable<string> albumIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(albumIds);

        var ids = string.Join(',', albumIds);
        if (ids.Length == 0)
        {
            return [];
        }

        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["album-ids"] = ids });
        var albums = await _connection
            .PostAsync<List<Album>>("/albums", content, cancellationToken)
            .ConfigureAwait(false);

        return albums ?? (IReadOnlyList<Album>)[];
    }

    /// <inheritdoc />
    public Task<Album?> GetWithTracksAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return _connection.GetAsync<Album>($"/albums/{Uri.EscapeDataString(albumId)}/with-tracks", cancellationToken);
    }

    /// <inheritdoc />
    public Task<AlbumSimilarEntities?> GetSimilarEntitiesAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return _connection.GetAsync<AlbumSimilarEntities>($"/albums/{Uri.EscapeDataString(albumId)}/similar-entities", cancellationToken);
    }

    /// <inheritdoc />
    public Task<AlbumTrailer?> GetTrailerAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        return _connection.GetAsync<AlbumTrailer>($"/albums/{Uri.EscapeDataString(albumId)}/trailer", cancellationToken);
    }
}
