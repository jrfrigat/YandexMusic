using System.Globalization;
using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Shows an album's tracklist and lets the user start playing from any track.</summary>
public sealed class AlbumScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the album screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public AlbumScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
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
            AnsiConsole.MarkupLine(Strings.AlbumNoTracks);
            return null;
        }

        var album = detail.Album;
        var year = album.Year?.ToString(CultureInfo.InvariantCulture) ?? "—";
        var title = $"[bold]{Markup.Escape(Format.Truncate(album.Title, 40))}[/] [grey]— {Markup.Escape(Format.Truncate(album.Artist, 24))} · {year}[/]";

        var picked = await new SelectionView<TrackView>(title, detail.Tracks, TrackListScreen.TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(detail.Tracks, TrackList.IndexOfId(detail.Tracks, picked.Id));
    }
}
