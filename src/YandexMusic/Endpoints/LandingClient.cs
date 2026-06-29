using YandexMusic.Http;
using YandexMusic.Models.Landing;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ILandingClient"/> implementation.</summary>
internal sealed class LandingClient : ILandingClient
{
    private const string EitherUserId = "10254713668400548221";

    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new landing endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public LandingClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Feed?> GetFeedAsync(CancellationToken cancellationToken = default)
    {
        return _connection.GetAsync<Feed>("/feed", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> GetFeedWizardPassedAsync(CancellationToken cancellationToken = default)
    {
        var status = await _connection
            .GetAsync<WizardStatus>("/feed/wizard/is-passed", cancellationToken)
            .ConfigureAwait(false);
        return status?.IsWizardPassed ?? false;
    }

    /// <inheritdoc />
    public Task<Landing?> GetAsync(IReadOnlyList<string> blocks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var blocksValue = string.Join(',', blocks);
        var url = $"/landing3?blocks={Uri.EscapeDataString(blocksValue)}&eitherUserId={EitherUserId}";
        return _connection.GetAsync<Landing>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ChartInfo?> GetChartAsync(string? chartOption = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(chartOption)
            ? "/landing3/chart"
            : $"/landing3/chart/{Uri.EscapeDataString(chartOption)}";
        return _connection.GetAsync<ChartInfo>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LandingList?> GetNewReleasesAsync(CancellationToken cancellationToken = default)
    {
        return _connection.GetAsync<LandingList>("/landing3/new-releases", cancellationToken);
    }

    /// <inheritdoc />
    public Task<LandingList?> GetNewPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        return _connection.GetAsync<LandingList>("/landing3/new-playlists", cancellationToken);
    }

    /// <inheritdoc />
    public Task<LandingList?> GetPodcastsAsync(CancellationToken cancellationToken = default)
    {
        return _connection.GetAsync<LandingList>("/landing3/podcasts", cancellationToken);
    }

    /// <inheritdoc />
    public Task<TagResult?> GetTagAsync(string tagId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tagId);
        return _connection.GetAsync<TagResult>($"/tags/{Uri.EscapeDataString(tagId)}/playlist-ids", cancellationToken);
    }
}
