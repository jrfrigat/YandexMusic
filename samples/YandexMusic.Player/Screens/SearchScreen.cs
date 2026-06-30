using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Searches for tracks and lets the user pick one to start playing (the whole result set queues).</summary>
public sealed class SearchScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the search screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public SearchScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
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

        var tracks = await AnsiConsole.Status()
            .StartAsync(Strings.Searching, _ => _catalog.SearchTracksAsync(query, cancellationToken))
            .ConfigureAwait(false);

        if (tracks.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.NothingFound);
            return null;
        }

        var picked = await new SelectionView<TrackView>(
                Strings.SearchResultsTitle(tracks.Count, Markup.Escape(query)),
                tracks,
                TrackListScreen.TrackConverter)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : new PlayRequest(tracks, TrackList.IndexOfId(tracks, picked.Id));
    }
}
