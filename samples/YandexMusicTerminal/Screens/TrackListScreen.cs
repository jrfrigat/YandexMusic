using Spectre.Console;
using YandexMusicTerminal.Catalog;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>
/// A reusable screen for a flat list of tracks (the "Liked" tracks and "My Wave"): it loads the
/// tracks, shows them in a <see cref="SelectionView{T}"/>, and plays the whole list from the chosen
/// track. <c>Esc</c> goes back.
/// </summary>
public sealed class TrackListScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly NoticeBoard _notices;

    /// <summary>Creates the track-list screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="notices">The board an empty list reports to.</param>
    public TrackListScreen(IMusicCatalog catalog, NoticeBoard notices)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(notices);
        _catalog = catalog;
        _notices = notices;
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
    public Task<PlayRequest?> RunWaveAsync(CancellationToken cancellationToken = default)
        => RunRadioAsync(_catalog.GetMyWaveAsync, cancellationToken);

    /// <summary>
    /// Lets the user pick a wave (My Wave or an activity) and its character, then shows the first
    /// batch. The character is a wave setting passed as an extra session seed, so the radio starts
    /// already tuned instead of being reconfigured first.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunTunedWaveAsync(CancellationToken cancellationToken = default)
    {
        var options = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingWaveSettings, _ => _catalog.GetWaveOptionsAsync(cancellationToken))
            .ConfigureAwait(false);

        IReadOnlyList<WaveChoice> waves = [new WaveChoice(Strings.MenuMyWave, "user:onyourwave"), .. options.Waves];
        var wave = await new SelectionView<WaveChoice>(Strings.PickWave, waves, ChoiceConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);
        if (wave is null)
        {
            return null;
        }

        var seeds = new List<string> { wave.Seed };
        if (options.Characters.Count > 0)
        {
            var character = await new SelectionView<WaveChoice>(Strings.PickWaveCharacter, options.Characters, ChoiceConverter)
                .ShowAsync(cancellationToken)
                .ConfigureAwait(false);
            if (character is null)
            {
                return null;
            }

            seeds.Add(character.Seed);
        }

        return await RunRadioAsync(ct => _catalog.StartRadioAsync(seeds, ct), cancellationToken).ConfigureAwait(false);
    }

    private async Task<PlayRequest?> RunRadioAsync(Func<CancellationToken, Task<RadioBatch>> start, CancellationToken cancellationToken)
    {
        var batch = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingWave, _ => start(cancellationToken))
            .ConfigureAwait(false);

        if (batch.Tracks.Count == 0)
        {
            _notices.Post(Strings.NoWave);
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
                TrackList.RadioOrigin("mywave", batch));
    }

    private async Task<PlayRequest?> RunTracksAsync(
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
            _notices.Post(emptyMessage);
            return null;
        }

        var picked = await new SelectionView<TrackView>(title(tracks.Count), tracks, TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(tracks, TrackList.IndexOfId(tracks, picked.Id), origin);
    }

    internal static string TrackConverter(TrackView track)
        => $"{Markup.Escape(Format.Truncate(track.Title, 42))} [grey]— {Markup.Escape(Format.Truncate(track.Artist, 24))}[/] [grey]({Format.Duration(track.Duration)})[/]";

    private static string ChoiceConverter(WaveChoice choice) => Markup.Escape(choice.Name);
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
            origin,
            ct => catalog.ResolveFallbackStreamUrlAsync(track.Id, ct));

    /// <summary>Builds the origin of a radio batch, so its items report to the batch's session.</summary>
    /// <param name="from">The analytics "from" label.</param>
    /// <param name="batch">The radio batch.</param>
    public static PlaybackOrigin RadioOrigin(string from, RadioBatch batch)
        => new(from, Station: batch.Station, BatchId: batch.BatchId, RadioSessionId: batch.SessionId, RadioKey: batch.SessionKey);

    /// <summary>
    /// Builds the queue continuation of a radio: every time the queue runs out it fetches the next
    /// batch of the same session.
    /// </summary>
    /// <param name="from">The analytics "from" label for the fetched items.</param>
    /// <param name="radioKey">The <see cref="RadioBatch.SessionKey"/> of the radio.</param>
    /// <param name="catalog">The catalog fetching the batches.</param>
    public static Func<CancellationToken, Task<IReadOnlyList<PlaybackItem>>> RadioContinuation(string from, string radioKey, IMusicCatalog catalog)
        => async ct =>
        {
            var batch = await catalog.GetNextRadioBatchAsync(radioKey, ct).ConfigureAwait(false);
            var origin = RadioOrigin(from, batch);
            return batch.Tracks.Select(t => ToPlaybackItem(t, origin, catalog)).ToList();
        };
}
