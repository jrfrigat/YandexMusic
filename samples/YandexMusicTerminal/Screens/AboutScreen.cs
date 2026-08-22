using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusicTerminal.Diagnostics;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>
/// What this build is and where it keeps its files, plus an update check on demand. The automatic
/// check runs on its own schedule and stays quiet; this screen is the place to ask right now and get
/// an answer either way, including the "you are already on the latest version" the notice line can
/// never show.
/// </summary>
public sealed class AboutScreen
{
    private const string RepositoryUrl = "https://github.com/jrfrigat/YandexMusic";

    private readonly UpdateChecker _updates;
    private readonly RequestLog _log;

    /// <summary>Creates the screen.</summary>
    /// <param name="updates">The update checker this screen displays and can trigger.</param>
    /// <param name="log">The request journal, whose state and path are shown.</param>
    public AboutScreen(UpdateChecker updates, RequestLog log)
    {
        ArgumentNullException.ThrowIfNull(updates);
        ArgumentNullException.ThrowIfNull(log);
        _updates = updates;
        _log = log;
    }

    /// <summary>Shows the screen until the user leaves it.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>
    /// <see langword="true"/> when the user asked to install the available update. Applying it ends
    /// the process, so the decision is handed back to the caller rather than acted on here.
    /// </returns>
    public async Task<bool> RunAsync(CancellationToken cancellationToken = default)
    {
        var exit = false;
        var update = false;

        await AnsiConsole.Live(Build())
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!exit && !cancellationToken.IsCancellationRequested)
                {
                    live.UpdateTarget(Build());

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.R:
                                if (!_updates.IsChecking)
                                {
                                    // Detached: the live view keeps redrawing and shows the answer
                                    // when it lands. CheckNowAsync never throws.
                                    _ = _updates.CheckNowAsync(cancellationToken);
                                }

                                break;
                            case ConsoleKey.U:
                                if (_updates.Available is not null && Updater.IsSupported)
                                {
                                    update = true;
                                    exit = true;
                                }

                                break;
                            case ConsoleKey.Q or ConsoleKey.Escape:
                                exit = true;
                                break;
                        }

                        if (exit)
                        {
                            break;
                        }
                    }

                    await Task.Delay(80, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

        return update;
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
        var rows = new List<IRenderable>
        {
            new Markup($"[white]ymt[/] [yellow]{Markup.Escape(UpdateChecker.CurrentVersion)}[/]"),
            new Markup($"[grey]{Markup.Escape(Strings.Subtitle)}[/]"),
            new Markup($"[grey]{Markup.Escape(RepositoryUrl)}[/]"),
            new Text(string.Empty),
            new Markup($"[grey]{Markup.Escape(Strings.AboutDataDirectory(AppPaths.DataDirectory))}[/]"),
            new Markup($"[grey]{Markup.Escape(_log.IsEnabled ? Strings.AboutJournalOn(_log.FilePath) : Strings.AboutJournalOff)}[/]"),
            new Text(string.Empty),
            new Markup(UpdateLine()),
        };

        var panel = new Panel(new Rows(rows))
            .Header(Strings.About)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(2, 1);

        return new Rows(panel, new Markup(Hotkeys()));
    }

    private string UpdateLine()
    {
        if (_updates.IsChecking)
        {
            return $"[blue]{Markup.Escape(Strings.AboutChecking)}[/]";
        }

        return _updates.Status switch
        {
            UpdateStatus.UpdateAvailable when _updates.Available is { } update
                => $"[yellow]{Markup.Escape(Strings.AboutUpdateFound(update.Version))}[/]",
            UpdateStatus.UpToDate => $"[green]{Markup.Escape(Strings.AboutUpToDate(UpdateChecker.CurrentVersion))}[/]",
            UpdateStatus.Failed => $"[red]{Markup.Escape(Strings.AboutCheckFailed)}[/]",
            _ => $"[grey]{Markup.Escape(Strings.AboutNotChecked)}[/]",
        };
    }

    private string Hotkeys()
        => _updates.Available is not null && Updater.IsSupported
            ? Strings.AboutHotkeys + Strings.AboutHotkeyUpdate
            : Strings.AboutHotkeys;
}
