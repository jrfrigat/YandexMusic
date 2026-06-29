using YandexMusic.Models.Disclaimers;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving the legal disclaimers attached to catalogue entities.</summary>
public interface IDisclaimersClient
{
    /// <summary>Gets the disclaimer for a track.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The disclaimer, or <see langword="null"/> if none applies.</returns>
    Task<Disclaimer?> GetTrackDisclaimerAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets the disclaimer for a clip.</summary>
    /// <param name="clipId">The clip identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The disclaimer, or <see langword="null"/> if none applies.</returns>
    Task<Disclaimer?> GetClipDisclaimerAsync(string clipId, CancellationToken cancellationToken = default);

    /// <summary>Gets the disclaimer for an album.</summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The disclaimer, or <see langword="null"/> if none applies.</returns>
    Task<Disclaimer?> GetAlbumDisclaimerAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Gets the disclaimer for an artist.</summary>
    /// <param name="artistId">The artist identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The disclaimer, or <see langword="null"/> if none applies.</returns>
    Task<Disclaimer?> GetArtistDisclaimerAsync(string artistId, CancellationToken cancellationToken = default);
}
