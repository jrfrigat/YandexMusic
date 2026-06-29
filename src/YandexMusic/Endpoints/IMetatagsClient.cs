using YandexMusic.Models.Metatags;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the metatag podborki (mood, activity, genre and epoch collections).</summary>
public interface IMetatagsClient
{
    /// <summary>Gets the full metatag tree (the moods, activities, genres and epochs sections).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The metatag tree, or <see langword="null"/> when unavailable.</returns>
    Task<Metatags?> GetTreeAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a single metatag landing page.</summary>
    /// <param name="metatagId">The metatag identifier (the <c>tag</c> value from a <see cref="MetatagLeaf"/>).</param>
    /// <param name="tracksCount">The number of tracks to include, when specified.</param>
    /// <param name="artistsCount">The number of artists to include, when specified.</param>
    /// <param name="composersCount">The number of composers to include, when specified.</param>
    /// <param name="albumsCount">The number of albums to include, when specified.</param>
    /// <param name="promotionsCount">The number of promotions to include, when specified.</param>
    /// <param name="featuresCount">The number of features to include, when specified.</param>
    /// <param name="playlistsCount">The number of playlists to include, when specified.</param>
    /// <param name="concertsCount">The number of concerts to include, when specified.</param>
    /// <param name="tracksSortBy">The tracks sort order (<c>popular</c> or <c>new</c>), when specified.</param>
    /// <param name="albumsSortBy">The albums sort order (<c>popular</c> or <c>new</c>), when specified.</param>
    /// <param name="withLikesCount">Whether to include like counts, when specified.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The metatag landing page, or <see langword="null"/> when not found.</returns>
    Task<Metatag?> GetAsync(
        string metatagId,
        int? tracksCount = null,
        int? artistsCount = null,
        int? composersCount = null,
        int? albumsCount = null,
        int? promotionsCount = null,
        int? featuresCount = null,
        int? playlistsCount = null,
        int? concertsCount = null,
        string? tracksSortBy = null,
        string? albumsSortBy = null,
        bool? withLikesCount = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paged albums listing for a metatag.</summary>
    /// <param name="metatagId">The metatag identifier.</param>
    /// <param name="period">The time period filter, when specified.</param>
    /// <param name="sortBy">The sort order (<c>popular</c> or <c>new</c>), when specified.</param>
    /// <param name="offset">The zero-based offset of the first item.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The albums listing, or <see langword="null"/> when not found.</returns>
    Task<MetatagAlbums?> GetAlbumsAsync(
        string metatagId,
        string? period = null,
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paged artists listing for a metatag.</summary>
    /// <param name="metatagId">The metatag identifier.</param>
    /// <param name="period">The time period filter (<c>week</c>, <c>month</c> or <c>day</c>); required by the API.</param>
    /// <param name="sortBy">The sort order (<c>popular</c>), when specified.</param>
    /// <param name="offset">The zero-based offset of the first item.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="tracksPerArtist">The number of popular tracks to include per artist, when specified.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The artists listing, or <see langword="null"/> when not found.</returns>
    Task<MetatagArtists?> GetArtistsAsync(
        string metatagId,
        string period = "week",
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        int? tracksPerArtist = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paged playlists listing for a metatag.</summary>
    /// <param name="metatagId">The metatag identifier.</param>
    /// <param name="sortBy">The sort order (<c>popular</c> or <c>new</c>), when specified.</param>
    /// <param name="offset">The zero-based offset of the first item.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="withLikesCount">Whether to include like counts, when specified.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The playlists listing, or <see langword="null"/> when not found.</returns>
    Task<MetatagPlaylists?> GetPlaylistsAsync(
        string metatagId,
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        bool? withLikesCount = null,
        CancellationToken cancellationToken = default);
}
