using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Clips;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IClipsClient"/> implementation.</summary>
internal sealed class ClipsClient : IClipsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new clips endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public ClipsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Clip>> GetManyAsync(IEnumerable<string> clipIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clipIds);

        var ids = string.Join(',', clipIds.Select(Uri.EscapeDataString));
        if (ids.Length == 0)
        {
            return [];
        }

        var clips = await _connection
            .GetAsync<List<Clip>>($"/clips?clipIds={ids}", cancellationToken)
            .ConfigureAwait(false);

        return clips ?? (IReadOnlyList<Clip>)[];
    }

    /// <inheritdoc />
    public Task<ClipsWillLike?> GetWillLikeAsync(int page = 0, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var url =
            $"/clips/will/like?page={page.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        return _connection.GetAsync<ClipsWillLike>(url, cancellationToken);
    }
}
