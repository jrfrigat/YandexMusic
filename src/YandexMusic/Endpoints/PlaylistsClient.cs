using System.Globalization;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Account;
using YandexMusic.Models.Playlists;
using YandexMusic.Serialization;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IPlaylistsClient"/> implementation.</summary>
internal sealed class PlaylistsClient : IPlaylistsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new playlists endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public PlaylistsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Playlist?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        return _connection.GetAsync<Playlist>(
            $"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Playlist>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var playlists = await _connection
            .GetAsync<List<Playlist>>($"/users/{Uri.EscapeDataString(userId)}/playlists/list", cancellationToken)
            .ConfigureAwait(false);

        return playlists ?? (IReadOnlyList<Playlist>)[];
    }

    /// <inheritdoc />
    public async Task<UserSettings?> GetUserSettingsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var response = await _connection
            .GetAsync<UserSettingsResponse>($"/users/{Uri.EscapeDataString(userId)}/settings", cancellationToken)
            .ConfigureAwait(false);

        return response?.UserSettings;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Playlist>> GetManyAsync(string userId, IReadOnlyList<string> kinds, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(kinds);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["kinds"] = string.Join(',', kinds),
        });

        var playlists = await _connection
            .PostAsync<List<Playlist>>($"/users/{Uri.EscapeDataString(userId)}/playlists", content, cancellationToken)
            .ConfigureAwait(false);

        return playlists ?? (IReadOnlyList<Playlist>)[];
    }

    /// <inheritdoc />
    public Task<PlaylistRecommendations?> GetRecommendationsAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        return _connection.GetAsync<PlaylistRecommendations>(
            $"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/recommendations",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Playlist?> CreateAsync(string userId, string title, PlaylistVisibility visibility = PlaylistVisibility.Public, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["title"] = title,
            ["visibility"] = ToWire(visibility),
        });

        return PostFormAsync($"/users/{Uri.EscapeDataString(userId)}/playlists/create", content, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        var result = await _connection
            .PostAsync<string>($"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/delete", null, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<Playlist?> RenameAsync(string userId, string kind, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["value"] = value });
        return PostFormAsync($"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/name", content, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Playlist?> SetVisibilityAsync(string userId, string kind, PlaylistVisibility visibility, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["value"] = ToWire(visibility) });
        return PostFormAsync($"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/visibility", content, cancellationToken);
    }

    // The visibility endpoints expect the lowercase wire token.
    private static string ToWire(PlaylistVisibility visibility)
        => visibility == PlaylistVisibility.Public ? "public" : "private";

    /// <inheritdoc />
    public Task<Playlist?> SetDescriptionAsync(string userId, string kind, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(value);

        var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["value"] = value });
        return PostFormAsync($"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/description", content, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Playlist?> ChangeAsync(string userId, string kind, string diff, int revision = 1, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(diff);

        return ChangeCoreAsync(userId, kind, diff, revision, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Playlist?> InsertTrackAsync(string userId, string kind, string trackId, string albumId, int at = 0, int revision = 1, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        var operations = new List<PlaylistDiffOperation>
        {
            new()
            {
                Op = "insert",
                At = at,
                Tracks = [new PlaylistDiffTrack { Id = trackId, AlbumId = albumId }],
            },
        };
        var diff = JsonSerializer.Serialize(operations, YandexMusicJson.TypeInfo<List<PlaylistDiffOperation>>());

        return ChangeCoreAsync(userId, kind, diff, revision, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Playlist?> DeleteTracksAsync(string userId, string kind, int from, int to, int revision = 1, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        var operations = new List<PlaylistDiffOperation>
        {
            new() { Op = "delete", From = from, To = to },
        };
        var diff = JsonSerializer.Serialize(operations, YandexMusicJson.TypeInfo<List<PlaylistDiffOperation>>());

        return ChangeCoreAsync(userId, kind, diff, revision, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> JoinCollectiveAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var url =
            $"/playlists/collective/join?uid={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
        var result = await _connection
            .PostAsync<string>(url, null, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<Playlist?> GetByUuidAsync(string playlistUuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUuid);

        return _connection.GetAsync<Playlist>($"/playlist/{Uri.EscapeDataString(playlistUuid)}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaylistSimilarEntities?> GetSimilarEntitiesAsync(string playlistUuid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUuid);

        return _connection.GetAsync<PlaylistSimilarEntities>(
            $"/playlist/{Uri.EscapeDataString(playlistUuid)}/similar-entities",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaylistsList?> GetByIdsAsync(IReadOnlyList<string> playlistIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlistIds);

        var ids = string.Join(',', playlistIds.Select(Uri.EscapeDataString));
        return _connection.GetAsync<PlaylistsList>($"/playlists?playlistIds={ids}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Playlist>> GetShortListAsync(IReadOnlyList<string> playlistIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlistIds);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["playlist-ids"] = string.Join(',', playlistIds),
        });

        var playlists = await _connection
            .PostAsync<List<Playlist>>("/playlists/list", content, cancellationToken)
            .ConfigureAwait(false);

        return playlists ?? (IReadOnlyList<Playlist>)[];
    }

    /// <inheritdoc />
    public Task<GeneratedPlaylist?> GetPersonalAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistId);

        return _connection.GetAsync<GeneratedPlaylist>($"/playlists/personal/{Uri.EscapeDataString(playlistId)}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlaylistTrailer?> GetTrailerAsync(string userId, string kind, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        return _connection.GetAsync<PlaylistTrailer>(
            $"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/trailer",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetKindsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var kinds = await _connection
            .GetAsync<List<string>>($"/users/{Uri.EscapeDataString(userId)}/playlists/list/kinds", cancellationToken)
            .ConfigureAwait(false);

        return kinds ?? (IReadOnlyList<string>)[];
    }

    private async Task<Playlist?> ChangeCoreAsync(string userId, string kind, string diff, int revision, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["kind"] = kind,
            ["revision"] = revision.ToString(CultureInfo.InvariantCulture),
            ["diff"] = diff,
        });

        return await _connection
            .PostAsync<Playlist>($"/users/{Uri.EscapeDataString(userId)}/playlists/{Uri.EscapeDataString(kind)}/change", content, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Playlist?> PostFormAsync(string relativeUrl, FormUrlEncodedContent content, CancellationToken cancellationToken)
    {
        using (content)
        {
            return await _connection
                .PostAsync<Playlist>(relativeUrl, content, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
