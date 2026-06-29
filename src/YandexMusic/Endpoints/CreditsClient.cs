using YandexMusic.Http;
using YandexMusic.Models.Credits;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ICreditsClient"/> implementation.</summary>
internal sealed class CreditsClient : ICreditsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new credits endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public CreditsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Credits?> GetTrackCreditsAsync(string trackId, CancellationToken cancellationToken = default)
        => GetCreditsAsync("tracks", trackId, cancellationToken);

    /// <inheritdoc />
    public Task<Credits?> GetClipCreditsAsync(string clipId, CancellationToken cancellationToken = default)
        => GetCreditsAsync("clips", clipId, cancellationToken);

    private Task<Credits?> GetCreditsAsync(string entityType, string entityId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        return _connection.GetAsync<Credits>($"/{entityType}/{Uri.EscapeDataString(entityId)}/credits", cancellationToken);
    }
}
