using YandexMusic.Http;
using YandexMusic.Models;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IGenresClient"/> implementation.</summary>
internal sealed class GenresClient : IGenresClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new genres endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public GenresClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _connection.GetAsync<List<Genre>>("/genres", cancellationToken).ConfigureAwait(false);
        return genres ?? (IReadOnlyList<Genre>)[];
    }
}
