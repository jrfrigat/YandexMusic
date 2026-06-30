using Spectre.Console;
using Spectre.Console.Rendering;

namespace YandexMusic.Player.Ui;

/// <summary>
/// A keyboard-driven selection list with a scrolling window and a hotkey bar. Unlike Spectre's
/// <c>SelectionPrompt</c> it supports <c>Esc</c> to go back from any screen and matches keys by
/// physical <see cref="ConsoleKey"/> (so navigation works on non-Latin keyboard layouts too).
/// Returns the chosen item, or <see langword="null"/> when the user pressed <c>Esc</c>.
/// </summary>
/// <typeparam name="T">The item type (a reference type).</typeparam>
public sealed class SelectionView<T>
    where T : class
{
    private readonly string _title;
    private readonly IReadOnlyList<T> _items;
    private readonly Func<T, string> _converter;
    private readonly int _pageSize;
    private int _index;
    private int _start;

    /// <summary>Creates a selection view.</summary>
    /// <param name="title">The header (may contain Spectre markup).</param>
    /// <param name="items">The items to choose from.</param>
    /// <param name="converter">Renders an item to a markup string.</param>
    /// <param name="pageSize">The number of rows visible at once.</param>
    public SelectionView(string title, IReadOnlyList<T> items, Func<T, string> converter, int pageSize = 15)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(converter);
        _title = title ?? string.Empty;
        _items = items;
        _converter = converter;
        _pageSize = Math.Max(3, pageSize);
    }

    /// <summary>Shows the list and returns the chosen item, or <see langword="null"/> on <c>Esc</c>.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task<T?> ShowAsync(CancellationToken cancellationToken = default)
    {
        if (_items.Count == 0)
        {
            return null;
        }

        T? result = null;
        var done = false;

        await AnsiConsole.Live(Build())
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!done && !cancellationToken.IsCancellationRequested)
                {
                    live.UpdateTarget(Build());

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.UpArrow or ConsoleKey.K:
                                Move(-1);
                                break;
                            case ConsoleKey.DownArrow or ConsoleKey.J:
                                Move(1);
                                break;
                            case ConsoleKey.PageUp:
                                Move(-_pageSize);
                                break;
                            case ConsoleKey.PageDown:
                                Move(_pageSize);
                                break;
                            case ConsoleKey.Home:
                                _index = 0;
                                break;
                            case ConsoleKey.End:
                                _index = _items.Count - 1;
                                break;
                            case ConsoleKey.Enter:
                                result = _items[_index];
                                done = true;
                                break;
                            case ConsoleKey.Escape:
                                result = null;
                                done = true;
                                break;
                        }

                        if (done)
                        {
                            break;
                        }
                    }

                    await Task.Delay(40, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

        return result;
    }

    private void Move(int delta) => _index = Math.Clamp(_index + delta, 0, _items.Count - 1);

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
        // Keep the cursor within the visible window.
        if (_index < _start)
        {
            _start = _index;
        }
        else if (_index >= _start + _pageSize)
        {
            _start = _index - _pageSize + 1;
        }

        _start = Math.Clamp(_start, 0, Math.Max(0, _items.Count - _pageSize));
        var end = Math.Min(_items.Count, _start + _pageSize);

        var rows = new List<IRenderable>();
        for (var i = _start; i < end; i++)
        {
            var text = _converter(_items[i]);
            rows.Add(new Markup(i == _index ? $"[green]▶[/] {text}" : $"  {text}"));
        }

        var panel = new Panel(new Rows(rows))
            .Header(_title)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey)
            .Padding(2, 1);

        var footer = $"{Strings.ListHotkeys}   [grey]{_index + 1}/{_items.Count}[/]";
        return new Rows(panel, new Markup(footer));
    }
}
