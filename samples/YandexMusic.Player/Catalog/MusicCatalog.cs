using System.Globalization;
using YandexMusic;
using YandexMusic.Endpoints;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Player.Catalog;

/// <summary>The default <see cref="IMusicCatalog"/> over an <see cref="IYandexMusicClient"/>.</summary>
public sealed class MusicCatalog : IMusicCatalog
{
    private const string MyWaveStation = "user:onyourwave";
    private const int MaxTrackBatch = 100;

    private readonly IYandexMusicClient _client;
    private string? _uid;

    /// <summary>Creates a catalog over the given client.</summary>
    /// <param name="client">The Yandex Music client.</param>
    public MusicCatalog(IYandexMusicClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <inheritdoc />
    public async Task<SearchPage<TrackView>> SearchTracksAsync(string query, int page = 0, CancellationToken cancellationToken = default)
    {
        var result = await _client.Search.SearchAsync(query, SearchType.Track, page, cancellationToken: cancellationToken).ConfigureAwait(false);
        var section = result?.Tracks;
        return new SearchPage<TrackView>((section?.Results ?? []).Select(ToTrackView).ToList(), section?.Total ?? 0);
    }

    /// <inheritdoc />
    public async Task<SearchPage<AlbumView>> SearchAlbumsAsync(string query, int page = 0, CancellationToken cancellationToken = default)
    {
        var result = await _client.Search.SearchAsync(query, SearchType.Album, page, cancellationToken: cancellationToken).ConfigureAwait(false);
        var section = result?.Albums;
        return new SearchPage<AlbumView>((section?.Results ?? []).Select(ToAlbumView).ToList(), section?.Total ?? 0);
    }

    /// <inheritdoc />
    public async Task<SearchPage<PlaylistView>> SearchPlaylistsAsync(string query, int page = 0, CancellationToken cancellationToken = default)
    {
        var result = await _client.Search.SearchAsync(query, SearchType.Playlist, page, cancellationToken: cancellationToken).ConfigureAwait(false);
        var section = result?.Playlists;
        return new SearchPage<PlaylistView>((section?.Results ?? []).Select(ToPlaylistView).ToList(), section?.Total ?? 0);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AlbumView>> GetMyAlbumsAsync(CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return [];
        }

        var liked = await _client.Library.GetLikedAlbumsAsync(uid, cancellationToken: cancellationToken).ConfigureAwait(false);
        return liked
            .Select(a => a.Album)
            .Where(a => a is not null)
            .Select(a => ToAlbumView(a!))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlaylistView>> GetMyPlaylistsAsync(CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return [];
        }

        var playlists = await _client.Playlists.GetByUserAsync(uid, cancellationToken).ConfigureAwait(false);
        return playlists.Select(ToPlaylistView).ToList();
    }

    /// <inheritdoc />
    public async Task<PlaylistDetail?> GetPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return null;
        }

        var playlist = await _client.Playlists.GetAsync(uid, playlistId, cancellationToken).ConfigureAwait(false);
        if (playlist is null)
        {
            return null;
        }

        var tracks = playlist.Tracks
            .Select(ToTrackView)
            .Where(t => t is not null)
            .Select(t => t!)
            .ToList();

        return new PlaylistDetail(ToPlaylistView(playlist), tracks);
    }

    /// <inheritdoc />
    public async Task<AlbumDetail?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default)
    {
        var album = await _client.Albums.GetWithTracksAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (album is null)
        {
            return null;
        }

        var tracks = (album.Volumes ?? [])
            .SelectMany(disc => disc)
            .Select(ToTrackView)
            .ToList();

        return new AlbumDetail(ToAlbumView(album), tracks);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TrackView>> GetLikedTracksAsync(CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return [];
        }

        var liked = await _client.Library.GetLikedTracksAsync(uid, cancellationToken: cancellationToken).ConfigureAwait(false);
        var ids = (liked?.Tracks ?? [])
            .Select(t => t.Id)
            .Where(id => !string.IsNullOrEmpty(id))
            .Take(MaxTrackBatch)
            .ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        // The liked list carries only ids, so fetch the metadata and restore the original order.
        var tracks = await _client.Tracks.GetManyAsync(ids, cancellationToken).ConfigureAwait(false);
        var byId = tracks.ToDictionary(t => t.Id, t => t);
        return ids
            .Select(id => byId.TryGetValue(id, out var track) ? ToTrackView(track) : null)
            .Where(v => v is not null)
            .Select(v => v!)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetLikedTrackIdsAsync(CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return [];
        }

        var liked = await _client.Library.GetLikedTracksAsync(uid, cancellationToken: cancellationToken).ConfigureAwait(false);
        return (liked?.Tracks ?? [])
            .Select(t => t.Id)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> SetTrackLikedAsync(string trackId, bool liked, CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return false;
        }

        var ids = new[] { trackId };
        return liked
            ? await _client.Library.AddLikedTracksAsync(uid, ids, cancellationToken).ConfigureAwait(false)
            : await _client.Library.RemoveLikedTracksAsync(uid, ids, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> DislikeTrackAsync(string trackId, CancellationToken cancellationToken = default)
    {
        var uid = await GetUidAsync(cancellationToken).ConfigureAwait(false);
        if (uid is null)
        {
            return false;
        }

        // A disliked track must not stay liked at the same time.
        await _client.Library.RemoveLikedTracksAsync(uid, new[] { trackId }, cancellationToken).ConfigureAwait(false);
        return await _client.Library.AddDislikedTracksAsync(uid, new[] { trackId }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<RadioBatch> GetMyWaveAsync(CancellationToken cancellationToken = default)
        => GetStationBatchAsync(MyWaveStation, cancellationToken);

    /// <inheritdoc />
    public Task<RadioBatch> GetRadioAsync(string station, CancellationToken cancellationToken = default)
        => GetStationBatchAsync(station, cancellationToken);

    /// <inheritdoc />
    public async Task<RadioBatch> GetSimilarRadioAsync(string trackId, CancellationToken cancellationToken = default)
        => await GetStationBatchAsync("track:" + trackId, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<string?> GetLyricsAsync(string trackId, CancellationToken cancellationToken = default)
    {
        // The signed /lyrics endpoint is currently rejected by the server ("Invalid Sign"), but the
        // supplement endpoint returns the text without any signing.
        var supplement = await _client.Tracks.GetSupplementAsync(trackId, cancellationToken: cancellationToken).ConfigureAwait(false);
        var lyrics = supplement?.Lyrics;
        if (lyrics is null)
        {
            return null;
        }

        return lyrics.FullText ?? lyrics.Text;
    }

    /// <inheritdoc />
    public Task<string?> ResolveStreamUrlAsync(string trackId, CancellationToken cancellationToken = default)
        => _client.Tracks.GetDirectLinkAsync(trackId, cancellationToken);

    private async Task<RadioBatch> GetStationBatchAsync(string station, CancellationToken cancellationToken)
    {
        var result = await _client.Radio.GetStationTracksAsync(station, cancellationToken: cancellationToken).ConfigureAwait(false);
        var tracks = (result?.Sequence ?? [])
            .Select(s => s.Track)
            .Where(t => t is not null)
            .Select(t => ToTrackView(t!))
            .ToList();
        var id = result?.Id;
        var stationId = id is { Type.Length: > 0, Tag.Length: > 0 } ? $"{id.Type}:{id.Tag}" : station;
        return new RadioBatch(tracks, stationId, result?.BatchId ?? string.Empty);
    }

    private async Task<string?> GetUidAsync(CancellationToken cancellationToken)
    {
        if (_uid is not null)
        {
            return _uid;
        }

        var status = await _client.Account.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        var uid = status?.Account.Uid ?? 0;
        return _uid = uid == 0 ? null : uid.ToString(CultureInfo.InvariantCulture);
    }

    private static TrackView ToTrackView(Track track) => new(
        track.Id,
        track.Title,
        JoinArtists(track.Artists),
        track.Albums.Count > 0 ? track.Albums[0].Title : null,
        TimeSpan.FromMilliseconds(track.DurationMs),
        track.Albums.Count > 0 ? track.Albums[0].Id : null);

    private static TrackView? ToTrackView(TrackShort trackShort)
    {
        // Playlist entries carry the full track when the playlist is fetched with rich tracks;
        // skip the rare entry that has no resolvable id.
        if (trackShort.Track is { } track)
        {
            return ToTrackView(track);
        }

        return string.IsNullOrEmpty(trackShort.Id) ? null : new TrackView(trackShort.Id, trackShort.Id, "Unknown", null, TimeSpan.Zero);
    }

    private static AlbumView ToAlbumView(Album album) => new(
        album.Id,
        album.Title,
        JoinArtists(album.Artists),
        album.Year,
        album.TrackCount);

    private static PlaylistView ToPlaylistView(Playlist playlist) => new(
        playlist.Kind.ToString(CultureInfo.InvariantCulture),
        playlist.Title,
        playlist.TrackCount);

    private static string JoinArtists(IReadOnlyList<Artist> artists)
        => artists.Count == 0 ? "Unknown" : string.Join(", ", artists.Select(a => a.Name));
}
