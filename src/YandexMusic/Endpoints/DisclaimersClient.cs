using YandexMusic.Http;
using YandexMusic.Models.Disclaimers;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IDisclaimersClient"/> implementation.</summary>
internal sealed class DisclaimersClient : IDisclaimersClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new disclaimers endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public DisclaimersClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Disclaimer?> GetTrackDisclaimerAsync(string trackId, CancellationToken cancellationToken = default)
        => GetDisclaimerAsync("tracks", trackId, cancellationToken);

    /// <inheritdoc />
    public Task<Disclaimer?> GetClipDisclaimerAsync(string clipId, CancellationToken cancellationToken = default)
        => GetDisclaimerAsync("clips", clipId, cancellationToken);

    /// <inheritdoc />
    public Task<Disclaimer?> GetAlbumDisclaimerAsync(string albumId, CancellationToken cancellationToken = default)
        => GetDisclaimerAsync("albums", albumId, cancellationToken);

    /// <inheritdoc />
    public Task<Disclaimer?> GetArtistDisclaimerAsync(string artistId, CancellationToken cancellationToken = default)
        => GetDisclaimerAsync("artists", artistId, cancellationToken);

    private Task<Disclaimer?> GetDisclaimerAsync(string entityType, string entityId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        return _connection.GetAsync<Disclaimer>($"/{entityType}/{Uri.EscapeDataString(entityId)}/disclaimer", cancellationToken);
    }
}
