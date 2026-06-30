using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Shows a playlist's tracklist and lets the user start playing from any track.</summary>
public sealed class PlaylistScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the playlist screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public PlaylistScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <summary>Runs the screen for a given playlist.</summary>
    /// <param name="playlistId">The playlist to show.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var detail = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingPlaylist, _ => _catalog.GetPlaylistAsync(playlistId, cancellationToken))
            .ConfigureAwait(false);

        if (detail is null || detail.Tracks.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.PlaylistNoTracks);
            return null;
        }

        var playlist = detail.Playlist;
        var title = $"[bold]{Markup.Escape(Format.Truncate(playlist.Title, 40))}[/] [grey]· {Strings.TracksSuffix(playlist.TrackCount)}[/]";

        var picked = await new SelectionView<TrackView>(title, detail.Tracks, TrackListScreen.TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(detail.Tracks, TrackList.IndexOfId(detail.Tracks, picked.Id));
    }
}
