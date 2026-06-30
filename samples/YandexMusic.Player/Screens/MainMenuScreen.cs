using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>The action chosen from the main menu.</summary>
public enum MainMenuAction
{
    /// <summary>Open the track search.</summary>
    Search,

    /// <summary>Open the user's albums.</summary>
    Albums,

    /// <summary>Open the user's playlists.</summary>
    Playlists,

    /// <summary>Open the now-playing view.</summary>
    NowPlaying,

    /// <summary>Sign out.</summary>
    SignOut,

    /// <summary>Quit the app.</summary>
    Quit,
}

/// <summary>
/// The main menu, rendered as a cursor-driven list with a persistent hotkey bar along the bottom.
/// Besides arrow-key navigation, every entry has a single-key shortcut (so <c>p</c> jumps straight to
/// the player from anywhere on the menu), and the currently-playing track is shown at the top.
/// </summary>
public sealed class MainMenuScreen
{
    private static readonly (MainMenuAction Action, string Label, char Key)[] Items =
    [
        (MainMenuAction.Search, "Search tracks", 's'),
        (MainMenuAction.Albums, "My albums", 'a'),
        (MainMenuAction.Playlists, "My playlists", 'l'),
        (MainMenuAction.NowPlaying, "Open player", 'p'),
        (MainMenuAction.SignOut, "Sign out", 'o'),
        (MainMenuAction.Quit, "Quit", 'q'),
    ];

    private readonly PlaybackController _controller;
    private int _index;

    /// <summary>Creates the main menu.</summary>
    /// <param name="controller">The playback controller, for the now-playing status line.</param>
    public MainMenuScreen(PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        _controller = controller;
    }

    /// <summary>Runs the menu until the user makes a choice.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>The chosen action.</returns>
    public async Task<MainMenuAction> RunAsync(CancellationToken cancellationToken = default)
    {
        var result = MainMenuAction.Quit;
        var chosen = false;

        await AnsiConsole.Live(Build())
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!chosen && !cancellationToken.IsCancellationRequested)
                {
                    live.UpdateTarget(Build());

                    while (TryReadKey(out var key, out var ch))
                    {
                        switch (key)
                        {
                            case ConsoleKey.UpArrow:
                                _index = (_index - 1 + Items.Length) % Items.Length;
                                break;
                            case ConsoleKey.DownArrow:
                                _index = (_index + 1) % Items.Length;
                                break;
                            case ConsoleKey.Enter:
                                result = Items[_index].Action;
                                chosen = true;
                                break;
                            case ConsoleKey.Escape:
                                result = MainMenuAction.Quit;
                                chosen = true;
                                break;
                            default:
                                var shortcut = Array.FindIndex(Items, i => i.Key == char.ToLowerInvariant(ch));
                                if (shortcut >= 0)
                                {
                                    _index = shortcut;
                                    result = Items[shortcut].Action;
                                    chosen = true;
                                }

                                break;
                        }

                        if (chosen)
                        {
                            break;
                        }
                    }

                    await Task.Delay(60, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

        return result;
    }

    private static bool TryReadKey(out ConsoleKey key, out char keyChar)
    {
        key = default;
        keyChar = default;
        try
        {
            if (!Console.KeyAvailable)
            {
                return false;
            }

            var info = Console.ReadKey(intercept: true);
            key = info.Key;
            keyChar = info.KeyChar;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private Rows Build()
    {
        var rows = new List<IRenderable> { new Markup(NowPlayingLine()), new Text(string.Empty) };

        for (var i = 0; i < Items.Length; i++)
        {
            var (_, label, key) = Items[i];
            rows.Add(new Markup(i == _index
                ? $"[green]▶[/] [white]{label}[/] [grey]({key})[/]"
                : $"  [grey]{label} ({key})[/]"));
        }

        var panel = new Panel(new Rows(rows))
            .Header("[yellow] Menu [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(2, 1);

        var hotkeys = "[grey]↑↓[/] move   [grey]enter[/] select   " +
                      "[grey]s[/] search   [grey]a[/] albums   [grey]l[/] playlists   " +
                      "[grey]p[/] open player   [grey]o[/] sign out   [grey]q[/] quit";

        return new Rows(panel, new Markup(hotkeys));
    }

    private string NowPlayingLine()
    {
        if (_controller.Current is not { } item)
        {
            return "[grey]♪ nothing playing[/]";
        }

        var state = _controller.State switch
        {
            PlaybackState.Playing => "[green]▶[/]",
            PlaybackState.Paused => "[yellow]⏸[/]",
            PlaybackState.Buffering => "[blue]…[/]",
            _ => "[grey]■[/]",
        };

        return $"{state} [white]{Markup.Escape(Format.Truncate(item.Title, 40))}[/] [grey]— {Markup.Escape(Format.Truncate(item.Artist, 30))}[/]";
    }
}
