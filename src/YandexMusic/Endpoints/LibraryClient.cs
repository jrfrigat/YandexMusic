using System.Globalization;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Clips;
using YandexMusic.Models.Library;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ILibraryClient"/> implementation.</summary>
internal sealed class LibraryClient : ILibraryClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new library endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public LibraryClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<LikedTracks?> GetLikedTracksAsync(string userId, int ifModifiedSinceRevision = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var url =
            $"/users/{Uri.EscapeDataString(userId)}/likes/tracks?if-modified-since-revision={ifModifiedSinceRevision.ToString(CultureInfo.InvariantCulture)}";
        var envelope = await _connection
            .GetAsync<LikedTracksEnvelope>(url, cancellationToken)
            .ConfigureAwait(false);

        return envelope?.Library;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LikedAlbum>> GetLikedAlbumsAsync(string userId, bool rich = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var albums = await _connection
            .GetAsync<List<LikedAlbum>>($"/users/{Uri.EscapeDataString(userId)}/likes/albums?rich={Bool(rich)}", cancellationToken)
            .ConfigureAwait(false);

        return albums ?? (IReadOnlyList<LikedAlbum>)[];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Artist>> GetLikedArtistsAsync(string userId, bool withTimestamps = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var artists = await _connection
            .GetAsync<List<Artist>>($"/users/{Uri.EscapeDataString(userId)}/likes/artists?with-timestamps={Bool(withTimestamps)}", cancellationToken)
            .ConfigureAwait(false);

        return artists ?? (IReadOnlyList<Artist>)[];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Like>> GetLikedPlaylistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var playlists = await _connection
            .GetAsync<List<Like>>($"/users/{Uri.EscapeDataString(userId)}/likes/playlists", cancellationToken)
            .ConfigureAwait(false);

        return playlists ?? (IReadOnlyList<Like>)[];
    }

    /// <inheritdoc />
    public Task<bool> AddLikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default)
        => MutateWithRevisionAsync(userId, "likes", "tracks", "add-multiple", "track-ids", trackIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveLikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default)
        => MutateWithRevisionAsync(userId, "likes", "tracks", "remove", "track-ids", trackIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> AddLikedAlbumsAsync(string userId, IEnumerable<string> albumIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "albums", "add-multiple", "album-ids", albumIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveLikedAlbumsAsync(string userId, IEnumerable<string> albumIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "albums", "remove", "album-ids", albumIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> AddLikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "artists", "add-multiple", "artist-ids", artistIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveLikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "artists", "remove", "artist-ids", artistIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> AddLikedPlaylistsAsync(string userId, IEnumerable<string> playlistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "playlists", "add-multiple", "playlist-ids", playlistIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveLikedPlaylistsAsync(string userId, IEnumerable<string> playlistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "likes", "playlists", "remove", "playlist-ids", playlistIds, cancellationToken);

    /// <inheritdoc />
    public async Task<LikedTracks?> GetDislikedTracksAsync(string userId, int ifModifiedSinceRevision = 0, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // The dislikes endpoint uses the underscore form of the revision key, unlike the likes endpoint.
        var url =
            $"/users/{Uri.EscapeDataString(userId)}/dislikes/tracks?if_modified_since_revision={ifModifiedSinceRevision.ToString(CultureInfo.InvariantCulture)}";
        var envelope = await _connection
            .GetAsync<LikedTracksEnvelope>(url, cancellationToken)
            .ConfigureAwait(false);

        return envelope?.Library;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Artist>> GetDislikedArtistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var artists = await _connection
            .GetAsync<List<Artist>>($"/users/{Uri.EscapeDataString(userId)}/dislikes/artists", cancellationToken)
            .ConfigureAwait(false);

        return artists ?? (IReadOnlyList<Artist>)[];
    }

    /// <inheritdoc />
    public Task<bool> AddDislikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default)
        => MutateWithRevisionAsync(userId, "dislikes", "tracks", "add-multiple", "track-ids", trackIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveDislikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default)
        => MutateWithRevisionAsync(userId, "dislikes", "tracks", "remove", "track-ids", trackIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> AddDislikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "dislikes", "artists", "add-multiple", "artist-ids", artistIds, cancellationToken);

    /// <inheritdoc />
    public Task<bool> RemoveDislikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default)
        => MutateWithOkAsync(userId, "dislikes", "artists", "remove", "artist-ids", artistIds, cancellationToken);

    /// <inheritdoc />
    public Task<ClipsWillLike?> GetLikedClipsAsync(string userId, int page = 0, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var url =
            $"/users/{Uri.EscapeDataString(userId)}/likes/clips?page={page.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        return _connection.GetAsync<ClipsWillLike>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AddLikedClipAsync(string userId, string clipId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clipId);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { ["clip-id"] = clipId });
        var result = await _connection
            .PostAsync<JsonElement>($"/users/{Uri.EscapeDataString(userId)}/likes/clips/add", content, cancellationToken)
            .ConfigureAwait(false);

        return IsClipMutationSuccess(result);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveLikedClipAsync(string userId, string clipId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clipId);

        var result = await _connection
            .PostAsync<JsonElement>($"/users/{Uri.EscapeDataString(userId)}/likes/clips/{Uri.EscapeDataString(clipId)}/remove", null, cancellationToken)
            .ConfigureAwait(false);

        return IsClipMutationSuccess(result);
    }

    private async Task<bool> MutateWithRevisionAsync(
        string userId,
        string scope,
        string type,
        string action,
        string bodyKey,
        IEnumerable<string> ids,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(ids);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { [bodyKey] = string.Join(',', ids) });
        var result = await _connection
            .PostAsync<LibraryRevisionResult>($"/users/{Uri.EscapeDataString(userId)}/{scope}/{type}/{action}", content, cancellationToken)
            .ConfigureAwait(false);

        return result?.Revision is not null;
    }

    private async Task<bool> MutateWithOkAsync(
        string userId,
        string scope,
        string type,
        string action,
        string bodyKey,
        IEnumerable<string> ids,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(ids);

        using var content = new FormUrlEncodedContent(new Dictionary<string, string> { [bodyKey] = string.Join(',', ids) });
        var result = await _connection
            .PostAsync<string>($"/users/{Uri.EscapeDataString(userId)}/{scope}/{type}/{action}", content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    // The API expects capitalized Python-style booleans ("True"/"False") in query strings.
    private static string Bool(bool value) => value ? "True" : "False";

    // Clip mutations succeed when the API returns either the literal "ok" or any object payload.
    private static bool IsClipMutationSuccess(JsonElement? result)
    {
        if (result is not { } element)
        {
            return false;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Object => true,
            JsonValueKind.String => string.Equals(element.GetString(), "ok", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }
}
