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
    public async Task<IReadOnlyList<TrackView>> SearchTracksAsync(string query, CancellationToken cancellationToken = default)
    {
        var result = await _client.Search.SearchAsync(query, SearchType.Track, cancellationToken: cancellationToken).ConfigureAwait(false);
        var tracks = result?.Tracks?.Results ?? [];
        return tracks.Select(ToTrackView).ToList();
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
    public Task<string?> ResolveStreamUrlAsync(string trackId, CancellationToken cancellationToken = default)
        => _client.Tracks.GetDirectLinkAsync(trackId, cancellationToken);

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
        TimeSpan.FromMilliseconds(track.DurationMs));

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
