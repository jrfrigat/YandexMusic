using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models.Artists;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IArtistsClient"/> implementation.</summary>
internal sealed class ArtistsClient : IArtistsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new artists endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public ArtistsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<ArtistBriefInfo?> GetBriefInfoAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistBriefInfo>(
            $"/artists/{Uri.EscapeDataString(artistId)}/brief-info",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistTracksPage?> GetTracksAsync(string artistId, int page = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var id = Uri.EscapeDataString(artistId);
        var p = page.ToString(CultureInfo.InvariantCulture);
        var size = pageSize.ToString(CultureInfo.InvariantCulture);
        return _connection.GetAsync<ArtistTracksPage>($"/artists/{id}/tracks?page={p}&page-size={size}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArtistAlbumsPage?> GetDirectAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sortBy);
        var id = Uri.EscapeDataString(artistId);
        var p = page.ToString(CultureInfo.InvariantCulture);
        var size = pageSize.ToString(CultureInfo.InvariantCulture);
        return _connection.GetAsync<ArtistAlbumsPage>(
            $"/artists/{id}/direct-albums?page={p}&page-size={size}&sort-by={Uri.EscapeDataString(sortBy)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArtistSimilar?> GetSimilarAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistSimilar>(
            $"/artists/{Uri.EscapeDataString(artistId)}/similar",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistLinks?> GetLinksAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistLinks>(
            $"/artists/{Uri.EscapeDataString(artistId)}/artist-links",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistAlbumsPage?> GetAlsoAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sortBy);
        var id = Uri.EscapeDataString(artistId);
        var p = page.ToString(CultureInfo.InvariantCulture);
        var size = pageSize.ToString(CultureInfo.InvariantCulture);
        return _connection.GetAsync<ArtistAlbumsPage>(
            $"/artists/{id}/also-albums?page={p}&page-size={size}&sort-by={Uri.EscapeDataString(sortBy)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArtistAlbumsPage?> GetDiscographyAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sortBy);
        var id = Uri.EscapeDataString(artistId);
        var p = page.ToString(CultureInfo.InvariantCulture);
        var size = pageSize.ToString(CultureInfo.InvariantCulture);
        return _connection.GetAsync<ArtistAlbumsPage>(
            $"/artists/{id}/discography-albums?page={p}&page-size={size}&sort-by={Uri.EscapeDataString(sortBy)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArtistAlbumsPage?> GetSafeDirectAlbumsAsync(string artistId, string sortBy = "year", string sortOrder = "desc", int limit = 20, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sortBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(sortOrder);
        var id = Uri.EscapeDataString(artistId);
        var lim = limit.ToString(CultureInfo.InvariantCulture);
        return _connection.GetAsync<ArtistAlbumsPage>(
            $"/artists/{id}/safe-direct-albums?sort-by={Uri.EscapeDataString(sortBy)}&sort-order={Uri.EscapeDataString(sortOrder)}&limit={lim}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetTrackIdsAsync(string artistId, int page = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var id = Uri.EscapeDataString(artistId);
        var p = page.ToString(CultureInfo.InvariantCulture);
        var size = pageSize.ToString(CultureInfo.InvariantCulture);
        var ids = await _connection.GetAsync<List<string>>(
            $"/artists/{id}/track-ids?page={p}&page-size={size}",
            cancellationToken).ConfigureAwait(false);
        return ids ?? [];
    }

    /// <inheritdoc />
    public Task<ArtistAbout?> GetAboutAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistAbout>(
            $"/artists/{Uri.EscapeDataString(artistId)}/about-artist",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistClips?> GetClipsAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistClips>(
            $"/artists/{Uri.EscapeDataString(artistId)}/blocks/artist-clips",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistDonations?> GetDonationAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistDonations>(
            $"/artists/{Uri.EscapeDataString(artistId)}/blocks/artist-donation",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistInfo?> GetInfoAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistInfo>(
            $"/artists/{Uri.EscapeDataString(artistId)}/info",
            cancellationToken);

    /// <inheritdoc />
    public Task<ArtistSkeleton?> GetSkeletonAsync(string artistId, string skeletonId = "web-artist-default", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(skeletonId);
        var id = Uri.EscapeDataString(artistId);
        return _connection.GetAsync<ArtistSkeleton>(
            $"/artists/{id}/skeletons/{Uri.EscapeDataString(skeletonId)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<ArtistTrailer?> GetTrailerAsync(string artistId, CancellationToken cancellationToken = default)
        => _connection.GetAsync<ArtistTrailer>(
            $"/artists/{Uri.EscapeDataString(artistId)}/trailer",
            cancellationToken);
}
