using YandexMusic.Models.Concerts;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the catalogue's concerts (afisha) data.</summary>
public interface IConcertsClient
{
    /// <summary>Gets the concerts associated with an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist's concerts, or <see langword="null"/> when not found.</returns>
    Task<ArtistConcerts?> GetArtistConcertsAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets detailed information about a single concert.</summary>
    /// <param name="concertId">The concert identifier (a UUID).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The concert information, or <see langword="null"/> when not found.</returns>
    Task<ConcertInfo?> GetInfoAsync(string concertId, CancellationToken cancellationToken = default);

    /// <summary>Gets the page skeleton (layout) for a concert page.</summary>
    /// <param name="concertId">The concert identifier (a UUID).</param>
    /// <param name="skeletonId">The skeleton identifier; defaults to <c>concert_page</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The concert page skeleton, or <see langword="null"/> when not found.</returns>
    Task<ConcertSkeleton?> GetSkeletonAsync(string concertId, string skeletonId = "concert_page", CancellationToken cancellationToken = default);

    /// <summary>Gets the concert feed, optionally filtered by location.</summary>
    /// <param name="locations">An optional list of geo identifiers to filter by; omitted when empty.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The concert feed, or <see langword="null"/> when not found.</returns>
    Task<ConcertFeed?> GetFeedAsync(IReadOnlyList<string>? locations = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the locations available for filtering the concert feed.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The available locations, or <see langword="null"/> when not found.</returns>
    Task<ConcertLocations?> GetLocationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the tab and pagination configuration for the concerts feed.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The tab configuration, or <see langword="null"/> when not found.</returns>
    Task<ConcertTabConfig?> GetTabConfigAsync(CancellationToken cancellationToken = default);
}
