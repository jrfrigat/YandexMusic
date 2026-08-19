using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// Catalogue search as a tabbed screen: a horizontal tab bar (tracks, albums, playlists) on top and
/// the active tab's result list below, in one live view. <c>←</c>/<c>→</c> (or <c>1-3</c>) switch
/// tabs, the list pages through a "more" row, and a picked album or playlist drills into its
/// tracklist. Each tab keeps its own loaded pages and cursor position.
/// </summary>
public sealed class SearchScreen
{
    private const int PageSize = 15;

    /// <summary>A row of a result list: a real item or the trailing "load more" sentinel.</summary>
    private sealed record Row(string Display, object? Item);

    /// <summary>The per-tab state: loaded pages, the paging cursor and the list cursor.</summary>
    private sealed class Tab
    {
        public required Func<int, CancellationToken, Task<SearchPage<Row>>> Load { get; init; }

        public List<Row> Rows { get; } = [];

        public int Page { get; set; } = -1;

        public int Total { get; set; }

        public bool Loading { get; set; }

        public int Cursor { get; set; }

        public int WindowStart { get; set; }
    }

    private readonly IMusicCatalog _catalog;
    private readonly AlbumScreen _albumScreen;
    private readonly PlaylistScreen _playlistScreen;

    /// <summary>Creates the search screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="albumScreen">The album detail screen to drill into.</param>
    /// <param name="playlistScreen">The playlist detail screen to drill into.</param>
    public SearchScreen(IMusicCatalog catalog, AlbumScreen albumScreen, PlaylistScreen playlistScreen)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(albumScreen);
        ArgumentNullException.ThrowIfNull(playlistScreen);
        _catalog = catalog;
        _albumScreen = albumScreen;
        _playlistScreen = playlistScreen;
    }

    /// <summary>Runs the screen.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(CancellationToken cancellationToken = default)
    {
        var query = AnsiConsole.Prompt(new TextPrompt<string>(Strings.SearchPrompt).AllowEmpty());
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var tabs = new[]
        {
            new Tab { Load = (page, ct) => LoadTracksAsync(query, page, ct) },
            new Tab { Load = (page, ct) => LoadAlbumsAsync(query, page, ct) },
            new Tab { Load = (page, ct) => LoadPlaylistsAsync(query, page, ct) },
        };
        var labels = new[] { Strings.SearchTabTracks, Strings.SearchTabAlbums, Strings.SearchTabPlaylists };
        var active = 0;
        PlayRequest? result = null;
        var exit = false;

        await AnsiConsole.Live(Build(tabs, labels, active, query))
            .AutoClear(true)
            .StartAsync(async live =>
            {
                while (!exit && !cancellationToken.IsCancellationRequested)
                {
                    live.UpdateTarget(Build(tabs, labels, active, query));

                    while (TryReadKey(out var key))
                    {
                        switch (key)
                        {
                            case ConsoleKey.LeftArrow when active > 0:
                                active--;
                                break;
                            case ConsoleKey.RightArrow when active < tabs.Length - 1:
                                active++;
                                break;
                            case ConsoleKey.D1 or ConsoleKey.NumPad1:
                                active = 0;
                                break;
                            case ConsoleKey.D2 or ConsoleKey.NumPad2:
                                active = 1;
                                break;
                            case ConsoleKey.D3 or ConsoleKey.NumPad3:
                                active = 2;
                                break;
                            case ConsoleKey.UpArrow or ConsoleKey.K:
                                MoveCursor(tabs[active], -1);
                                break;
                            case ConsoleKey.DownArrow or ConsoleKey.J:
                                MoveCursor(tabs[active], +1);
                                break;
                            case ConsoleKey.Enter:
                                result = await ActivateAsync(tabs[active], active, cancellationToken).ConfigureAwait(false);
                                if (result is not null)
                                {
                                    exit = true;
                                }
                                else
                                {
                                    // A drill-in (album/playlist view) needed the console; re-render.
                                    live.UpdateTarget(Build(tabs, labels, active, query));
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

                    if (!exit)
                    {
                        await Task.Delay(60, cancellationToken).ConfigureAwait(false);
                    }
                }
            }).ConfigureAwait(false);

        return result;
    }

    private async Task<SearchPage<Row>> LoadTracksAsync(string query, int page, CancellationToken cancellationToken)
    {
        var result = await _catalog.SearchTracksAsync(query, page, cancellationToken).ConfigureAwait(false);
        return new SearchPage<Row>(result.Items.Select(t => new Row(TrackListScreen.TrackConverter(t), t)).ToList(), result.Total);
    }

    private async Task<SearchPage<Row>> LoadAlbumsAsync(string query, int page, CancellationToken cancellationToken)
    {
        var result = await _catalog.SearchAlbumsAsync(query, page, cancellationToken).ConfigureAwait(false);
        return new SearchPage<Row>(result.Items.Select(a => new Row(AlbumRow(a), a)).ToList(), result.Total);
    }

    private async Task<SearchPage<Row>> LoadPlaylistsAsync(string query, int page, CancellationToken cancellationToken)
    {
        var result = await _catalog.SearchPlaylistsAsync(query, page, cancellationToken).ConfigureAwait(false);
        return new SearchPage<Row>(result.Items.Select(p => new Row(PlaylistRow(p), p)).ToList(), result.Total);
    }

    private async Task<PlayRequest?> ActivateAsync(Tab tab, int tabIndex, CancellationToken cancellationToken)
    {
        // Fetch the first page lazily, right before it is needed.
        await EnsureLoadedAsync(tab, cancellationToken).ConfigureAwait(false);
        if (tab.Rows.Count == 0 || tab.Cursor >= tab.Rows.Count)
        {
            return null;
        }

        var row = tab.Rows[tab.Cursor];
        if (row.Item is null)
        {
            // The "load more" sentinel.
            await LoadNextPageAsync(tab, cancellationToken).ConfigureAwait(false);
            return null;
        }

        if (tabIndex == 0)
        {
            var tracks = tab.Rows.TakeWhile(r => r.Item is not null).Select(r => (TrackView)r.Item!).ToList();
            return new PlayRequest(tracks, tab.Cursor);
        }

        // The drill-in screens own the console while open; the live view re-renders afterwards.
        return tabIndex == 1
            ? await _albumScreen.RunAsync(((AlbumView)row.Item).Id, cancellationToken).ConfigureAwait(false)
            : await _playlistScreen.RunAsync(((PlaylistView)row.Item).Id, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureLoadedAsync(Tab tab, CancellationToken cancellationToken)
    {
        if (tab.Page >= 0)
        {
            return;
        }

        await LoadNextPageAsync(tab, cancellationToken).ConfigureAwait(false);
    }

    private static async Task LoadNextPageAsync(Tab tab, CancellationToken cancellationToken)
    {
        tab.Loading = true;
        try
        {
            var page = await tab.Load(tab.Page + 1, cancellationToken).ConfigureAwait(false);
            tab.Rows.RemoveAll(r => r.Item is null);
            if (page.Items.Count > 0)
            {
                tab.Page++;
                tab.Total = page.Total;
                tab.Rows.AddRange(page.Items);
            }
            else
            {
                tab.Total = tab.Rows.Count;
            }

            if (tab.Rows.Count < tab.Total)
            {
                tab.Rows.Add(new Row(Strings.LoadMore, null));
            }

            tab.Cursor = Math.Clamp(tab.Cursor, 0, Math.Max(0, tab.Rows.Count - 1));
        }
        finally
        {
            tab.Loading = false;
        }
    }

    private static void MoveCursor(Tab tab, int delta)
    {
        if (tab.Rows.Count == 0)
        {
            return;
        }

        tab.Cursor = (tab.Cursor + delta + tab.Rows.Count) % tab.Rows.Count;
        if (tab.Cursor < tab.WindowStart)
        {
            tab.WindowStart = tab.Cursor;
        }
        else if (tab.Cursor >= tab.WindowStart + PageSize)
        {
            tab.WindowStart = tab.Cursor - PageSize + 1;
        }
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

    private static Layout Build(Tab[] tabs, string[] labels, int active, string query)
    {
        var layout = new Layout("root")
            .SplitRows(
                new Layout("tabs").Size(2),
                new Layout("content").MinimumSize(3));

        layout["tabs"].Update(
            new Markup(
                "  " + string.Join("   ", labels.Select((label, i) => i == active
                    ? $"[black on green] {label} [/]"
                    : $"[grey]{label}[/]")) +
                $"  [grey]— {Markup.Escape(Format.Truncate(query, 30))}[/]"));

        var tab = tabs[active];
        var rows = new List<IRenderable>();
        if (tab.Loading)
        {
            rows.Add(new Markup($"[grey]{Strings.LoadingMore}[/]"));
        }
        else if (tab.Rows.Count == 0)
        {
            rows.Add(new Markup($"[grey]{Strings.NothingFound}[/]"));
        }
        else
        {
            var windowStart = Math.Max(0, Math.Min(tab.WindowStart, Math.Max(0, tab.Rows.Count - PageSize)));
            for (var i = windowStart; i < tab.Rows.Count && i < windowStart + PageSize; i++)
            {
                var row = tab.Rows[i];
                var display = row.Item is null ? $"[yellow]{Strings.LoadMore}[/]" : row.Display;
                rows.Add(new Markup(i == tab.Cursor ? $"[green]▶[/] {display}" : $"  {display}"));
            }
        }

        rows.Add(new Markup(string.Empty));
        rows.Add(new Markup(Strings.SearchTabsKeys));

        layout["content"].Update(new Rows(rows));
        return layout;
    }

    private static string AlbumRow(AlbumView album)
        => $"{Markup.Escape(Format.Truncate(album.Title, 40))} [grey]— {Markup.Escape(Format.Truncate(album.Artist, 24))}" +
           (album.Year is { } year ? $" ({year})" : string.Empty) + "[/]";

    private static string PlaylistRow(PlaylistView playlist)
        => $"{Markup.Escape(Format.Truncate(playlist.Title, 50))} [grey]— {playlist.TrackCount}[/]";
}
