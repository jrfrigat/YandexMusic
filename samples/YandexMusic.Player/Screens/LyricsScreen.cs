using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// A scrollable lyrics view for a track. The text arrives as plain lines (no timestamps), so the
/// view simply pages through it; the track keeps playing in the background.
/// </summary>
public sealed class LyricsScreen
{
    private const int VisibleLines = 18;

    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the lyrics screen.</summary>
    /// <param name="catalog">The catalog to fetch lyrics from.</param>
    public LyricsScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <summary>Shows the lyrics of a track until the user presses <c>q</c>/<c>Esc</c>.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(string trackId, CancellationToken cancellationToken = default)
    {
        string? text = null;
        await AnsiConsole.Status()
            .StartAsync(Strings.LyricsLoading, async _ => text = await _catalog.GetLyricsAsync(trackId, cancellationToken))
            .ConfigureAwait(false);

        var lines = (text ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n');
        if (lines.Length == 0 || string.IsNullOrWhiteSpace(string.Concat(lines)))
        {
            AnsiConsole.MarkupLine(Strings.LyricsUnavailable);
            return;
        }

        var offset = 0;
        var exit = false;
        await AnsiConsole.Live(Build(lines, offset))
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!exit && !cancellationToken.IsCancellationRequested)
                {
                    live.UpdateTarget(Build(lines, offset));

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.UpArrow or ConsoleKey.K:
                                offset = Math.Max(0, offset - 1);
                                break;
                            case ConsoleKey.DownArrow or ConsoleKey.J:
                                offset = Math.Min(Math.Max(0, lines.Length - VisibleLines), offset + 1);
                                break;
                            case ConsoleKey.PageUp:
                                offset = Math.Max(0, offset - VisibleLines);
                                break;
                            case ConsoleKey.PageDown or ConsoleKey.Spacebar:
                                offset = Math.Min(Math.Max(0, lines.Length - VisibleLines), offset + VisibleLines);
                                break;
                            case ConsoleKey.Q or ConsoleKey.Escape:
                                exit = true;
                                break;
                        }
                    }

                    await Task.Delay(80, cancellationToken).ConfigureAwait(false);
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
            return false;
        }
    }

    private static Panel Build(string[] lines, int offset)
    {
        var rows = new List<IRenderable>();
        foreach (var line in lines.Skip(offset).Take(VisibleLines))
        {
            rows.Add(new Markup(string.IsNullOrWhiteSpace(line)
                ? string.Empty
                : $"[white]{Markup.Escape(line.Trim())}[/]"));
        }

        return new Panel(new Rows(rows))
            .Header(Strings.LyricsHeader)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue)
            .Padding(2, 1);
    }
}
