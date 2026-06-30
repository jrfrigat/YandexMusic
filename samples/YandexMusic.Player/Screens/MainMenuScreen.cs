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

    /// <summary>Open the user's liked tracks.</summary>
    Liked,

    /// <summary>Open "My Wave".</summary>
    MyWave,

    /// <summary>Open the now-playing view.</summary>
    NowPlaying,

    /// <summary>Sign out.</summary>
    SignOut,

    /// <summary>Quit the app.</summary>
    Quit,
}

/// <summary>
/// The main menu, rendered as a cursor-driven list with a persistent hotkey bar along the bottom.
/// Besides arrow-key navigation, every entry has a single-key shortcut matched by physical
/// <see cref="ConsoleKey"/> (so they work on non-Latin keyboard layouts too — pressing the key in the
/// "p" position opens the player regardless of layout). The currently-playing track is shown on top.
/// </summary>
public sealed class MainMenuScreen
{
    private static readonly (MainMenuAction Action, ConsoleKey Key, char Hint)[] Items =
    [
        (MainMenuAction.Search, ConsoleKey.S, 's'),
        (MainMenuAction.Albums, ConsoleKey.A, 'a'),
        (MainMenuAction.Playlists, ConsoleKey.L, 'l'),
        (MainMenuAction.Liked, ConsoleKey.F, 'f'),
        (MainMenuAction.MyWave, ConsoleKey.W, 'w'),
        (MainMenuAction.NowPlaying, ConsoleKey.P, 'p'),
        (MainMenuAction.SignOut, ConsoleKey.O, 'o'),
        (MainMenuAction.Quit, ConsoleKey.Q, 'q'),
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

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.UpArrow or ConsoleKey.K:
                                _index = (_index - 1 + Items.Length) % Items.Length;
                                break;
                            case ConsoleKey.DownArrow or ConsoleKey.J:
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
                                var shortcut = Array.FindIndex(Items, item => item.Key == key);
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
            return false;
        }
    }

    private Rows Build()
    {
        var rows = new List<IRenderable> { new Markup(NowPlayingLine()), new Text(string.Empty) };

        for (var i = 0; i < Items.Length; i++)
        {
            var (action, _, hint) = Items[i];
            var label = LabelFor(action);
            rows.Add(new Markup(i == _index
                ? $"[green]▶[/] [white]{label}[/] [grey]({hint})[/]"
                : $"  [grey]{label} ({hint})[/]"));
        }

        var panel = new Panel(new Rows(rows))
            .Header(Strings.Menu)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(2, 1);

        return new Rows(panel, new Markup(Strings.MenuHotkeys));
    }

    private static string LabelFor(MainMenuAction action) => action switch
    {
        MainMenuAction.Search => Strings.MenuSearch,
        MainMenuAction.Albums => Strings.MenuAlbums,
        MainMenuAction.Playlists => Strings.MenuPlaylists,
        MainMenuAction.Liked => Strings.MenuLiked,
        MainMenuAction.MyWave => Strings.MenuMyWave,
        MainMenuAction.NowPlaying => Strings.MenuOpenPlayer,
        MainMenuAction.SignOut => Strings.MenuSignOut,
        MainMenuAction.Quit => Strings.MenuQuit,
        _ => action.ToString(),
    };

    private string NowPlayingLine()
    {
        if (_controller.Current is not { } item)
        {
            return Strings.NothingPlaying;
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
