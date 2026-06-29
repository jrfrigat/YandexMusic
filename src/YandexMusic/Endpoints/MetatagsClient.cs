using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models.Metatags;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IMetatagsClient"/> implementation.</summary>
internal sealed class MetatagsClient : IMetatagsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new metatags endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public MetatagsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Metatags?> GetTreeAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<Metatags>("/landing3/metatags", cancellationToken);

    /// <inheritdoc />
    public Task<Metatag?> GetAsync(
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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metatagId);

        var query = new List<string>();
        AppendInt(query, "tracksCount", tracksCount);
        AppendInt(query, "artistsCount", artistsCount);
        AppendInt(query, "composersCount", composersCount);
        AppendInt(query, "albumsCount", albumsCount);
        AppendInt(query, "promotionsCount", promotionsCount);
        AppendInt(query, "featuresCount", featuresCount);
        AppendInt(query, "playlistsCount", playlistsCount);
        AppendInt(query, "concertsCount", concertsCount);
        AppendString(query, "tracksSortBy", tracksSortBy);
        AppendString(query, "albumsSortBy", albumsSortBy);
        AppendBool(query, "withLikesCount", withLikesCount);

        return _connection.GetAsync<Metatag>(
            $"/metatags/{Uri.EscapeDataString(metatagId)}{BuildQuery(query)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<MetatagAlbums?> GetAlbumsAsync(
        string metatagId,
        string? period = null,
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metatagId);

        var query = new List<string>();
        AppendString(query, "period", period);
        AppendString(query, "sortBy", sortBy);
        AppendInt(query, "offset", offset);
        AppendInt(query, "limit", limit);

        return _connection.GetAsync<MetatagAlbums>(
            $"/metatags/{Uri.EscapeDataString(metatagId)}/albums{BuildQuery(query)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<MetatagArtists?> GetArtistsAsync(
        string metatagId,
        string period = "week",
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        int? tracksPerArtist = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metatagId);
        ArgumentException.ThrowIfNullOrWhiteSpace(period);

        var query = new List<string>();
        AppendString(query, "period", period);
        AppendString(query, "sortBy", sortBy);
        AppendInt(query, "offset", offset);
        AppendInt(query, "limit", limit);
        AppendInt(query, "tracksPerArtist", tracksPerArtist);

        return _connection.GetAsync<MetatagArtists>(
            $"/metatags/{Uri.EscapeDataString(metatagId)}/artists{BuildQuery(query)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<MetatagPlaylists?> GetPlaylistsAsync(
        string metatagId,
        string? sortBy = null,
        int offset = 0,
        int limit = 25,
        bool? withLikesCount = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metatagId);

        var query = new List<string>();
        AppendString(query, "sortBy", sortBy);
        AppendInt(query, "offset", offset);
        AppendInt(query, "limit", limit);
        AppendBool(query, "withLikesCount", withLikesCount);

        return _connection.GetAsync<MetatagPlaylists>(
            $"/metatags/{Uri.EscapeDataString(metatagId)}/playlists{BuildQuery(query)}",
            cancellationToken);
    }

    private static void AppendInt(List<string> query, string key, int value)
        => query.Add($"{key}={value.ToString(CultureInfo.InvariantCulture)}");

    private static void AppendInt(List<string> query, string key, int? value)
    {
        if (value is { } v)
        {
            query.Add($"{key}={v.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static void AppendString(List<string> query, string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            query.Add($"{key}={Uri.EscapeDataString(value)}");
        }
    }

    private static void AppendBool(List<string> query, string key, bool? value)
    {
        if (value is { } v)
        {
            query.Add($"{key}={(v ? "true" : "false")}");
        }
    }

    private static string BuildQuery(List<string> query)
        => query.Count == 0 ? string.Empty : "?" + string.Join('&', query);
}
