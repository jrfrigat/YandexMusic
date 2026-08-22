using Spectre.Console;
using YandexMusicTerminal.Catalog;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>Shows an artist's popular tracks and lets the user start playing from any of them.</summary>
public sealed class ArtistScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly NoticeBoard _notices;

    /// <summary>Creates the artist screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="notices">The board an artist without playable tracks reports to.</param>
    public ArtistScreen(IMusicCatalog catalog, NoticeBoard notices)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(notices);
        _catalog = catalog;
        _notices = notices;
    }

    /// <summary>Runs the screen for a given artist.</summary>
    /// <param name="artistId">The artist to show.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(string artistId, CancellationToken cancellationToken = default)
    {
        var detail = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingArtist, _ => _catalog.GetArtistAsync(artistId, cancellationToken))
            .ConfigureAwait(false);

        if (detail is null || detail.Tracks.Count == 0)
        {
            _notices.Post(Strings.ArtistNoTracks);
            return null;
        }

        var artist = detail.Artist;
        var title = $"[bold]{Markup.Escape(Format.Truncate(artist.Name, 40))}[/] [grey]· {Strings.TracksSuffix(detail.Tracks.Count)}[/]";

        var picked = await new SelectionView<TrackView>(title, detail.Tracks, TrackListScreen.TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null
            ? null
            : new PlayRequest(
                detail.Tracks,
                TrackList.IndexOfId(detail.Tracks, picked.Id),
                new PlaybackOrigin("artist"));
    }
}
