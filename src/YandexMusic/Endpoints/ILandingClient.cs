using YandexMusic.Models;
using YandexMusic.Models.Landing;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the personalized landing page, feed, charts, and curated lists.</summary>
public interface ILandingClient
{
    /// <summary>Gets the personalized feed, including smart playlists and the event timeline.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The feed, or <see langword="null"/> when unavailable.</returns>
    Task<Feed?> GetFeedAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets whether the current user has completed the onboarding wizard.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the wizard has been completed.</returns>
    Task<bool> GetFeedWizardPassedAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the landing page assembled from the requested blocks.</summary>
    /// <param name="blocks">The block names to include (for example <c>personalplaylists</c>, <c>chart</c>, <c>mixes</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The landing payload, or <see langword="null"/> when unavailable.</returns>
    Task<Landing?> GetAsync(IReadOnlyList<string> blocks, CancellationToken cancellationToken = default);

    /// <summary>Gets the chart for the optional territory option.</summary>
    /// <param name="chartOption">
    /// The optional territory postfix taken from a <see cref="ChartInfoMenuItem.Url"/> (for example
    /// <c>russia</c> or <c>world</c>). When <see langword="null"/> or empty, the default chart is returned.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The chart payload, or <see langword="null"/> when unavailable.</returns>
    Task<ChartInfo?> GetChartAsync(string? chartOption = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the list of new album releases.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The new-releases list, or <see langword="null"/> when unavailable.</returns>
    Task<LandingList?> GetNewReleasesAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the list of new playlists.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The new-playlists list, or <see langword="null"/> when unavailable.</returns>
    Task<LandingList?> GetNewPlaylistsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the list of featured podcasts.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The podcasts list, or <see langword="null"/> when unavailable.</returns>
    Task<LandingList?> GetPodcastsAsync(CancellationToken cancellationToken = default);

    /// <summary>Resolves a tag to its playlist references.</summary>
    /// <param name="tagId">The tag identifier, for example <c>belarus</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The tag result, or <see langword="null"/> when unavailable.</returns>
    Task<TagResult?> GetTagAsync(string tagId, CancellationToken cancellationToken = default);
}
