using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// The live "now playing" view: an animated equalizer, a progress bar that advances in real time, a
/// volume meter and keyboard transport controls. It renders the <see cref="PlaybackController"/>'s
/// state and translates key presses into transport commands.
/// </summary>
public sealed class NowPlayingScreen
{
    private const string EqualizerBlocks = "▁▂▃▄▅▆▇█";

    private readonly PlaybackController _controller;
    private int _frame;

    /// <summary>Creates the now-playing screen.</summary>
    /// <param name="controller">The playback controller to render and drive.</param>
    public NowPlayingScreen(PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controller = controller;
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

        var exit = false;
        await AnsiConsole.Live(Build())
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!exit && !cancellationToken.IsCancellationRequested)
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
                            case ConsoleKey.Q or ConsoleKey.Escape:
                                exit = true;
                                break;
                        }
                    }

                    await Task.Delay(120, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
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
            new Markup(StatusLine()),
            new Markup(ProgressLine(position, duration)),
            new Markup(VolumeLine()),
            new Markup($"[grey]{Strings.TrackCounter(_controller.QueuePosition, _controller.QueueLength)}{(_controller.ProducesSound ? string.Empty : Strings.SimulatedSuffix)}[/]"),
            new Markup(Strings.NowPlayingKeys),
        };

        return new Panel(new Rows(rows))
            .Header(Strings.NowPlayingHeader)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Padding(2, 1);
    }

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
