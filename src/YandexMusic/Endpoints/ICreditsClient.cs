using YandexMusic.Models.Credits;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving the production credits of tracks and clips.</summary>
public interface ICreditsClient
{
    /// <summary>Gets the production credits of a track.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The credits, or <see langword="null"/> if none are available.</returns>
    Task<Credits?> GetTrackCreditsAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets the production credits of a clip.</summary>
    /// <param name="clipId">The clip identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The credits, or <see langword="null"/> if none are available.</returns>
    Task<Credits?> GetClipCreditsAsync(string clipId, CancellationToken cancellationToken = default);
}
