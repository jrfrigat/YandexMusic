using System.Text;
using YandexMusic;
using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// The live "now playing" view: an animated equalizer, a progress bar that advances in real time, a
/// volume meter, keyboard transport controls, and the per-track actions — like, dislike, lyrics and
/// the "similar tracks" radio. It renders the <see cref="PlaybackController"/>'s state and
/// translates key presses into commands.
/// </summary>
public sealed class NowPlayingScreen
{
    private const string EqualizerBlocks = "▁▂▃▄▅▆▇█";
    private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(4);

    private readonly PlaybackController _controller;
    private readonly IMusicCatalog _catalog;
    private readonly LyricsScreen _lyrics;
    private HashSet<string> _likedIds = [];
    private bool _likedLoaded;
    private string _toast = string.Empty;
    private DateTime _toastShownAt;
    private int _frame;

    /// <summary>Creates the now-playing screen.</summary>
    /// <param name="controller">The playback controller to render and drive.</param>
    /// <param name="catalog">The catalog for like/dislike, lyrics and similar-radio actions.</param>
    /// <param name="lyrics">The lyrics view opened by the <c>t</c> key.</param>
    public NowPlayingScreen(PlaybackController controller, IMusicCatalog catalog, LyricsScreen lyrics)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(lyrics);
        _controller = controller;
        _catalog = catalog;
        _lyrics = lyrics;
    }

    /// <summary>What the live view asked for when it closed.</summary>
    private enum ViewExit
    {
        /// <summary>The user pressed <c>q</c>/<c>Esc</c> — leave the screen.</summary>
        Back,

        /// <summary>The lyrics view was requested; reopen the live view afterwards.</summary>
        Lyrics,
    }

    /// <summary>Runs the live view until the user presses <c>q</c>/<c>Esc</c>.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_controller.Current is null)
        {
            AnsiConsole.MarkupLine(Strings.NothingPlayingYet);
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var exit = await RunViewAsync(cancellationToken).ConfigureAwait(false);
            if (exit != ViewExit.Lyrics)
            {
                return;
            }

            if (_controller.Current is { } item
                && !await _lyrics.RunAsync(item.Id, cancellationToken).ConfigureAwait(false))
            {
                // The live view reopens right below; the refusal belongs in its toast, not in the
                // scrollback above it.
                ShowToast(Strings.LyricsUnavailable);
            }
        }
    }

    private async Task<ViewExit> RunViewAsync(CancellationToken cancellationToken)
    {
        var exit = ViewExit.Back;
        await AnsiConsole.Live(Build())
            .AutoClear(true)
            .StartAsync(async live =>
            {
                var running = true;
                while (running && !cancellationToken.IsCancellationRequested)
                {
                    _frame++;
                    live.UpdateTarget(Build());

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.Spacebar or ConsoleKey.P:
                                _controller.TogglePause();
                                break;
                            case ConsoleKey.RightArrow or ConsoleKey.N:
                                await _controller.NextAsync(cancellationToken).ConfigureAwait(false);
                                break;
                            case ConsoleKey.LeftArrow or ConsoleKey.B:
                                await _controller.PreviousAsync(cancellationToken).ConfigureAwait(false);
                                break;
                            case ConsoleKey.UpArrow or ConsoleKey.Add or ConsoleKey.OemPlus:
                                _controller.AdjustVolume(5);
                                break;
                            case ConsoleKey.DownArrow or ConsoleKey.Subtract or ConsoleKey.OemMinus:
                                _controller.AdjustVolume(-5);
                                break;
                            case ConsoleKey.S:
                                _controller.Stop();
                                break;
                            case ConsoleKey.L:
                                await ToggleLikeAsync(cancellationToken).ConfigureAwait(false);
                                break;
                            case ConsoleKey.X:
                                await DislikeAsync(cancellationToken).ConfigureAwait(false);
                                break;
                            case ConsoleKey.T:
                                // The lyrics view needs the console; close the live display first.
                                exit = ViewExit.Lyrics;
                                running = false;
                                break;
                            case ConsoleKey.I:
                                await StartSimilarRadioAsync(cancellationToken).ConfigureAwait(false);
                                break;
                            case ConsoleKey.Q or ConsoleKey.Escape:
                                running = false;
                                break;
                        }

                        if (!running)
                        {
                            break;
                        }
                    }

                    if (running)
                    {
                        await Task.Delay(120, cancellationToken).ConfigureAwait(false);
                    }
                }
            }).ConfigureAwait(false);

        return exit;
    }

    private void ShowToast(string message)
    {
        _toast = message;
        _toastShownAt = DateTime.UtcNow;
    }

    private async Task EnsureLikedLoadedAsync(CancellationToken cancellationToken)
    {
        if (_likedLoaded)
        {
            return;
        }

        try
        {
            _likedIds = [.. await _catalog.GetLikedTrackIdsAsync(cancellationToken).ConfigureAwait(false)];
            _likedLoaded = true;
        }
        catch (YandexMusicException)
        {
            // No like markers this run — the actions below still work, just without the indicator.
        }
    }

    private async Task ToggleLikeAsync(CancellationToken cancellationToken)
    {
        if (_controller.Current is not { } item)
        {
            return;
        }

        await EnsureLikedLoadedAsync(cancellationToken).ConfigureAwait(false);
        var liked = !_likedIds.Contains(item.Id);
        try
        {
            if (await _catalog.SetTrackLikedAsync(item.Id, liked, cancellationToken).ConfigureAwait(false))
            {
                if (liked)
                {
                    _ = _likedIds.Add(item.Id);
                }
                else
                {
                    _ = _likedIds.Remove(item.Id);
                }

                ShowToast(liked ? Strings.LikeAdded : Strings.LikeRemoved);
            }
            else
            {
                ShowToast(Strings.ActionFailed);
            }
        }
        catch (YandexMusicException)
        {
            ShowToast(Strings.ActionFailed);
        }
    }

    private async Task DislikeAsync(CancellationToken cancellationToken)
    {
        if (_controller.Current is not { } item)
        {
            return;
        }

        await EnsureLikedLoadedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (await _catalog.DislikeTrackAsync(item.Id, cancellationToken).ConfigureAwait(false))
            {
                _ = _likedIds.Remove(item.Id);
                ShowToast(Strings.DislikeDone);
                await _controller.NextAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                ShowToast(Strings.ActionFailed);
            }
        }
        catch (YandexMusicException)
        {
            ShowToast(Strings.ActionFailed);
        }
    }

    private async Task StartSimilarRadioAsync(CancellationToken cancellationToken)
    {
        if (_controller.Current is not { } item)
        {
            return;
        }

        ShowToast(Strings.SimilarStarting);
        try
        {
            var batch = await _catalog.GetSimilarRadioAsync(item.Id, cancellationToken).ConfigureAwait(false);
            if (batch.Tracks.Count == 0)
            {
                ShowToast(Strings.NothingFound);
                return;
            }

            var origin = new PlaybackOrigin("similar", Station: batch.Station, BatchId: batch.BatchId);
            var items = batch.Tracks.Select(t => TrackList.ToPlaybackItem(t, origin, _catalog)).ToList();
            await _controller.PlayAsync(
                items,
                continuation: ct => FetchRadioItemsAsync(batch.Station, ct),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (YandexMusicException)
        {
            ShowToast(Strings.ActionFailed);
        }
    }

    private async Task<IReadOnlyList<PlaybackItem>> FetchRadioItemsAsync(string station, CancellationToken cancellationToken)
    {
        var batch = await _catalog.GetRadioAsync(station, cancellationToken).ConfigureAwait(false);
        var origin = new PlaybackOrigin("similar", Station: batch.Station, BatchId: batch.BatchId);
        return batch.Tracks.Select(t => TrackList.ToPlaybackItem(t, origin, _catalog)).ToList();
    }

    private static bool TryReadKey(out ConsoleKey key)
    {
        key = default;
        try
        {
            if (!Console.KeyAvailable)
            {
                return false;
            }

            key = Console.ReadKey(intercept: true).Key;
            return true;
        }
        catch (InvalidOperationException)
        {
            // Input is redirected — no interactive keys.
            return false;
        }
    }

    private Panel Build()
    {
        var item = _controller.Current!;
        var position = _controller.Position;
        var duration = _controller.Duration;

        var rows = new List<IRenderable>
        {
            new Markup($"[bold white]{Markup.Escape(Format.Truncate(item.Title, 60))}[/]"),
            new Markup($"[grey]{Markup.Escape(Format.Truncate(item.Artist, 60))}[/]"),
            new Markup(LikeLine(item)),
            new Markup(StatusLine()),
            new Markup(ProgressLine(position, duration)),
            new Markup(VolumeLine()),
            new Markup($"[grey]{Strings.TrackCounter(_controller.QueuePosition, _controller.QueueLength)}{(_controller.ProducesSound ? string.Empty : Strings.SimulatedSuffix)}[/]"),
        };
        if (!string.IsNullOrEmpty(_toast) && DateTime.UtcNow - _toastShownAt < ToastLifetime)
        {
            rows.Add(new Markup($"[grey]{Markup.Escape(_toast)}[/]"));
        }

        rows.Add(new Markup(Strings.NowPlayingKeys));

        return new Panel(new Rows(rows))
            .Header(Strings.NowPlayingHeader)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Padding(2, 1);
    }

    private string LikeLine(PlaybackItem item)
        => _likedIds.Contains(item.Id) ? $"[red]{Strings.LikeMarker}[/]" : $"[grey]{Strings.NotLikedMarker}[/]";

    private string StatusLine() => _controller.State switch
    {
        PlaybackState.Playing => $"{Strings.StatePlaying}   {Equalizer()}",
        PlaybackState.Paused => Strings.StatePaused,
        PlaybackState.Buffering => Strings.StateBuffering,
        PlaybackState.Stopped => Strings.StateStopped,
        PlaybackState.Ended => Strings.StateEnded,
        PlaybackState.Error => Strings.StateError,
        _ => Strings.StateIdle,
    };

    private string Equalizer()
    {
        var builder = new StringBuilder();
        for (var i = 0; i < 7; i++)
        {
            var height = (int)(Math.Abs(Math.Sin((_frame + (i * 3)) * 0.45)) * (EqualizerBlocks.Length - 1));
            builder.Append(EqualizerBlocks[height]);
        }

        return $"[green]{builder}[/]";
    }

    private static string ProgressLine(TimeSpan position, TimeSpan duration)
    {
        const int width = 44;
        var fraction = duration.Ticks > 0 ? Math.Clamp((double)position.Ticks / duration.Ticks, 0, 1) : 0;
        var filled = (int)(fraction * width);
        var bar = $"[green]{new string('━', filled)}[/][grey]{new string('━', width - filled)}[/]";
        return $"{bar}  [grey]{Format.Duration(position)} / {Format.Duration(duration)}[/]";
    }

    private string VolumeLine()
    {
        const int width = 12;
        var filled = _controller.Volume * width / 100;
        return $"[grey]{Strings.VolumeLabel}[/] [green]{new string('█', filled)}[/][grey]{new string('░', width - filled)}[/] [grey]{_controller.Volume,3}%[/]";
    }
}
