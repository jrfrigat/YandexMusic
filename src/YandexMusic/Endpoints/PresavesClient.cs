using YandexMusic.Http;
using YandexMusic.Models.Presaves;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IPresavesClient"/> implementation.</summary>
internal sealed class PresavesClient : IPresavesClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new pre-saves endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public PresavesClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Presaves?> GetAsync(
        string userId,
        bool includeReleased = false,
        bool includeUpcoming = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var url =
            $"/users/{Uri.EscapeDataString(userId)}/presaves?includeReleased={(includeReleased ? "true" : "false")}&includeUpcoming={(includeUpcoming ? "true" : "false")}";

        return _connection.GetAsync<Presaves>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AddAsync(
        string userId,
        string albumId,
        bool likeAfterRelease = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        var fields = new Dictionary<string, string>
        {
            ["albumId"] = albumId,
            ["likeAfterRelease"] = likeAfterRelease ? "true" : "false",
        };

        using var content = new FormUrlEncodedContent(fields);
        var result = await _connection
            .PostAsync<string>($"/users/{Uri.EscapeDataString(userId)}/presaves/add", content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string userId, string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        var fields = new Dictionary<string, string>
        {
            ["albumId"] = albumId,
        };

        using var content = new FormUrlEncodedContent(fields);
        var result = await _connection
            .PostAsync<string>($"/users/{Uri.EscapeDataString(userId)}/presaves/remove", content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }
}
