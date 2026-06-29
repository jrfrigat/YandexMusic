using YandexMusic.Models.Artists;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving artists from the catalogue.</summary>
public interface IArtistsClient
{
    /// <summary>
    /// Gets a consolidated view of an artist: the artist together with their albums, popular tracks
    /// and similar artists.
    /// </summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist's brief info, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistBriefInfo?> GetBriefInfoAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of an artist's tracks.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of tracks per page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested page of tracks, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistTracksPage?> GetTracksAsync(string artistId, int page = 0, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of an artist's directly-released albums.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of albums per page.</param>
    /// <param name="sortBy">The sort order (for example <c>year</c> or <c>rating</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested page of albums, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistAlbumsPage?> GetDirectAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default);

    /// <summary>Gets the artists similar to a given artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The similar-artists result, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistSimilar?> GetSimilarAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets the external links of an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist's links, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistLinks?> GetLinksAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of albums the artist also appears on.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of albums per page.</param>
    /// <param name="sortBy">The sort order (for example <c>year</c> or <c>rating</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested page of albums, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistAlbumsPage?> GetAlsoAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default);

    /// <summary>Gets a page of an artist's discography albums.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of albums per page.</param>
    /// <param name="sortBy">The sort order (for example <c>year</c> or <c>rating</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested page of albums, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistAlbumsPage?> GetDiscographyAlbumsAsync(string artistId, int page = 0, int pageSize = 20, string sortBy = "year", CancellationToken cancellationToken = default);

    /// <summary>Gets a limited page of an artist's directly-released albums, sorted explicitly.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="sortBy">The sort field (for example <c>year</c>).</param>
    /// <param name="sortOrder">The sort direction (<c>asc</c> or <c>desc</c>).</param>
    /// <param name="limit">The maximum number of albums to return.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested page of albums, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistAlbumsPage?> GetSafeDirectAlbumsAsync(string artistId, string sortBy = "year", string sortOrder = "desc", int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>Gets the identifiers of an artist's tracks.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of identifiers per page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The track identifiers; an empty list if the artist has none.</returns>
    Task<IReadOnlyList<string>> GetTrackIdsAsync(string artistId, int page = 0, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>Gets a descriptive profile of an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist profile, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistAbout?> GetAboutAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of an artist's clips.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist's clips, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistClips?> GetClipsAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets the donation entries advertised for an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist's donation entries, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistDonations?> GetDonationAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets a rich profile of an artist, including statistics and trailer availability.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist profile, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistInfo?> GetInfoAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Gets the placeholder layout (skeleton) of an artist landing page.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="skeletonId">The skeleton identifier (defaults to the web artist layout).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist page skeleton, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistSkeleton?> GetSkeletonAsync(string artistId, string skeletonId = "web-artist-default", CancellationToken cancellationToken = default);

    /// <summary>Gets the trailer attached to an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artist trailer, or <see langword="null"/> if the artist does not exist.</returns>
    Task<ArtistTrailer?> GetTrailerAsync(string artistId, CancellationToken cancellationToken = default);
}
