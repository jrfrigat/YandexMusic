using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>
/// Catalogue search with tabbed categories (tracks, albums, playlists), paging via a "load more"
/// row, and drill-in: a picked album or playlist opens its tracklist through the dedicated screens.
/// </summary>
public sealed class SearchScreen
{
    /// <summary>A row of a result list: either a real item or the "load more" sentinel.</summary>
    private sealed record Row<T>(string Display, T? Item)
        where T : class;

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
            Strings.SearchTabTracks,
            Strings.SearchTabAlbums,
            Strings.SearchTabPlaylists,
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            var tab = await new SelectionView<string>(Strings.SearchTabsTitle, tabs, t => t)
                .ShowAsync(cancellationToken)
                .ConfigureAwait(false);

            if (tab is null)
            {
                return null;
            }

            PlayRequest? request;
            if (tab == tabs[0])
            {
                request = await BrowseAsync(
                    query,
                    (page, ct) => _catalog.SearchTracksAsync(query, page, ct),
                    TrackListScreen.TrackConverter,
                    (_, tracks, index, ct) => Task.FromResult<PlayRequest?>(new PlayRequest(tracks, index, new PlaybackOrigin("search"))),
                    cancellationToken).ConfigureAwait(false);
            }
            else if (tab == tabs[1])
            {
                request = await BrowseAsync(
                    query,
                    (page, ct) => _catalog.SearchAlbumsAsync(query, page, ct),
                    AlbumRow,
                    (album, _, _, ct) => _albumScreen.RunAsync(album.Id, ct),
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                request = await BrowseAsync(
                    query,
                    (page, ct) => _catalog.SearchPlaylistsAsync(query, page, ct),
                    PlaylistRow,
                    (playlist, _, _, ct) => _playlistScreen.RunAsync(playlist.Id, ct),
                    cancellationToken).ConfigureAwait(false);
            }

            // Null means "Esc" — back to the tab picker; a request starts playback.
            if (request is not null)
            {
                return request;
            }
        }

        return null;
    }

    private static async Task<PlayRequest?> BrowseAsync<T>(
        string query,
        Func<int, CancellationToken, Task<SearchPage<T>>> load,
        Func<T, string> row,
        Func<T, IReadOnlyList<T>, int, CancellationToken, Task<PlayRequest?>> open,
        CancellationToken cancellationToken)
        where T : class
    {
        var items = new List<T>();
        var page = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await AnsiConsole.Status()
                .StartAsync(Strings.Searching, _ => load(page, cancellationToken))
                .ConfigureAwait(false);
            if (result.Items.Count == 0)
            {
                if (items.Count == 0)
                {
                    AnsiConsole.MarkupLine(Strings.NothingFound);
                }

                return null;
            }

            items.AddRange(result.Items);
            var picked = await PickAsync(
                Strings.SearchResultsTitle(result.Total, Markup.Escape(query)),
                items.Select(i => new Row<T>(row(i), i)).ToList(),
                result.Total,
                cancellationToken).ConfigureAwait(false);
            if (picked is null)
            {
                return null;
            }

            if (picked.Item is { } item)
            {
                return await open(item, items, items.IndexOf(item), cancellationToken).ConfigureAwait(false);
            }

            // The "load more" sentinel — it only appears while more pages remain.
            page++;
        }

        return null;
    }

    private static async Task<Row<T>?> PickAsync<T>(
        string title,
        List<Row<T>> rows,
        int total,
        CancellationToken cancellationToken)
        where T : class
    {
        if (rows.Count < total)
        {
            rows.Add(new Row<T>($"[yellow]{Strings.LoadMore}[/]", null));
        }

        return await new SelectionView<Row<T>>(title, rows, r => r.Display, pageSize: 18)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private static string AlbumRow(AlbumView album)
        => $"{Markup.Escape(Format.Truncate(album.Title, 40))} [grey]— {Markup.Escape(Format.Truncate(album.Artist, 24))}" +
           (album.Year is { } year ? $" ({year})" : string.Empty) + "[/]";

    private static string PlaylistRow(PlaylistView playlist)
        => $"{Markup.Escape(Format.Truncate(playlist.Title, 50))} [grey]— {playlist.TrackCount}[/]";
}
