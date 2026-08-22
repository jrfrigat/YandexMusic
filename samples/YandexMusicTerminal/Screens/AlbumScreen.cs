using System.Globalization;
using Spectre.Console;
using YandexMusicTerminal.Catalog;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>Shows an album's tracklist and lets the user start playing from any track.</summary>
public sealed class AlbumScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly NoticeBoard _notices;

    /// <summary>Creates the album screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="notices">The board an empty album reports to.</param>
    public AlbumScreen(IMusicCatalog catalog, NoticeBoard notices)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(notices);
        _catalog = catalog;
        _notices = notices;
    }

    /// <summary>Runs the screen for a given album.</summary>
    /// <param name="albumId">The album to show.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(string albumId, CancellationToken cancellationToken = default)
    {
        var detail = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingAlbum, _ => _catalog.GetAlbumAsync(albumId, cancellationToken))
            .ConfigureAwait(false);

        if (detail is null || detail.Tracks.Count == 0)
        {
            _notices.Post(Strings.AlbumNoTracks);
            return null;
        }

        var album = detail.Album;
        var year = album.Year?.ToString(CultureInfo.InvariantCulture) ?? "—";
        var title = $"[bold]{Markup.Escape(Format.Truncate(album.Title, 40))}[/] [grey]— {Markup.Escape(Format.Truncate(album.Artist, 24))} · {year}[/]";

        var picked = await new SelectionView<TrackView>(title, detail.Tracks, TrackListScreen.TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(detail.Tracks, TrackList.IndexOfId(detail.Tracks, picked.Id), new PlaybackOrigin("album", AlbumId: albumId));
    }
}
