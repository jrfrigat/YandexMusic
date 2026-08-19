using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// A reusable screen for a flat list of tracks (the "Liked" tracks and "My Wave"): it loads the
/// tracks, shows them in a <see cref="SelectionView{T}"/>, and plays the whole list from the chosen
/// track. <c>Esc</c> goes back.
/// </summary>
public sealed class TrackListScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the track-list screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public TrackListScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <summary>Shows the user's liked tracks.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public Task<PlayRequest?> RunLikedAsync(CancellationToken cancellationToken = default)
        => RunTracksAsync(
            Strings.LoadingLiked,
            _catalog.GetLikedTracksAsync,
            Strings.LikedTitle,
            Strings.NoLiked,
            new PlaybackOrigin("library"),
            cancellationToken);

    /// <summary>Shows a batch of tracks from "My Wave".</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunWaveAsync(CancellationToken cancellationToken = default)
    {
        var batch = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingWave, _ => _catalog.GetMyWaveAsync(cancellationToken))
            .ConfigureAwait(false);

        if (batch.Tracks.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.NoWave);
            return null;
        }

        var picked = await new SelectionView<TrackView>(Strings.WaveTitle(batch.Tracks.Count), batch.Tracks, TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null
            ? null
            : new PlayRequest(
                batch.Tracks,
                TrackList.IndexOfId(batch.Tracks, picked.Id),
                new PlaybackOrigin("mywave", Station: batch.Station, BatchId: batch.BatchId));
    }

    private static async Task<PlayRequest?> RunTracksAsync(
        string loadingMessage,
        Func<CancellationToken, Task<IReadOnlyList<TrackView>>> loader,
        Func<int, string> title,
        string emptyMessage,
        PlaybackOrigin origin,
        CancellationToken cancellationToken)
    {
        var tracks = await AnsiConsole.Status()
            .StartAsync(loadingMessage, _ => loader(cancellationToken))
            .ConfigureAwait(false);

        if (tracks.Count == 0)
        {
            AnsiConsole.MarkupLine(emptyMessage);
            return null;
        }

        var picked = await new SelectionView<TrackView>(title(tracks.Count), tracks, TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(tracks, TrackList.IndexOfId(tracks, picked.Id), origin);
    }

    internal static string TrackConverter(TrackView track)
        => $"{Markup.Escape(Format.Truncate(track.Title, 42))} [grey]— {Markup.Escape(Format.Truncate(track.Artist, 24))}[/] [grey]({Format.Duration(track.Duration)})[/]";
}

/// <summary>Small helpers shared by the track-bearing screens.</summary>
internal static class TrackList
{
    /// <summary>Returns the index of the track with the given id, or 0 when not found.</summary>
    /// <param name="tracks">The tracks to search.</param>
    /// <param name="id">The track id to find.</param>
    public static int IndexOfId(IReadOnlyList<TrackView> tracks, string id)
    {
        for (var i = 0; i < tracks.Count; i++)
        {
            if (tracks[i].Id == id)
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>Maps a catalog track to a playable item with a lazily-resolved stream URL.</summary>
    /// <param name="track">The track to map.</param>
    /// <param name="origin">Where the track's queue came from, for play reporting.</param>
    /// <param name="catalog">The catalog resolving the stream URL.</param>
    public static PlaybackItem ToPlaybackItem(TrackView track, PlaybackOrigin? origin, IMusicCatalog catalog)
        => new(
            track.Id,
            track.Title,
            track.Artist,
            track.Duration,
            ct => catalog.ResolveStreamUrlAsync(track.Id, ct),
            origin);
}
