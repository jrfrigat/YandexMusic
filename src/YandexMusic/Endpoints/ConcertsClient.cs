using YandexMusic.Http;
using YandexMusic.Models.Concerts;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IConcertsClient"/> implementation.</summary>
internal sealed class ConcertsClient : IConcertsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new concerts endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public ConcertsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<ArtistConcerts?> GetArtistConcertsAsync(string artistId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);
        return _connection.GetAsync<ArtistConcerts>(
            $"/artists/{Uri.EscapeDataString(artistId)}/concerts",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConcertInfo?> GetInfoAsync(string concertId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concertId);
        return _connection.GetAsync<ConcertInfo>(
            $"/concerts/{Uri.EscapeDataString(concertId)}/info",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConcertSkeleton?> GetSkeletonAsync(string concertId, string skeletonId = "concert_page", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concertId);
        ArgumentException.ThrowIfNullOrWhiteSpace(skeletonId);
        return _connection.GetAsync<ConcertSkeleton>(
            $"/concerts/{Uri.EscapeDataString(concertId)}/skeletons/{Uri.EscapeDataString(skeletonId)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConcertFeed?> GetFeedAsync(IReadOnlyList<string>? locations = null, CancellationToken cancellationToken = default)
    {
        var url = "/concerts/feed";
        if (locations is { Count: > 0 })
        {
            url += $"?locations={string.Join(',', locations.Select(Uri.EscapeDataString))}";
        }

        return _connection.GetAsync<ConcertFeed>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConcertLocations?> GetLocationsAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<ConcertLocations>("/concerts/locations", cancellationToken);

    /// <inheritdoc />
    public Task<ConcertTabConfig?> GetTabConfigAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<ConcertTabConfig>("/concerts/tab-config", cancellationToken);
}
