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
        var query = AnsiConsole.Prompt(
            new TextPrompt<string>("[yellow]Search tracks[/] [grey](empty to go back)[/]:").AllowEmpty());
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var tracks = await AnsiConsole.Status()
            .StartAsync("Searching…", _ => _catalog.SearchTracksAsync(query, cancellationToken))
            .ConfigureAwait(false);

        if (tracks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Nothing found.[/]");
            return null;
        }

        var back = new TrackView(BackId, "← Back", string.Empty, null, TimeSpan.Zero);
        var choices = new List<TrackView>(tracks) { back };

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<TrackView>()
                .Title($"[green]{tracks.Count}[/] results for [yellow]{Markup.Escape(query)}[/]:")
                .PageSize(15)
                .MoreChoicesText("[grey](move up/down for more)[/]")
                .UseConverter(t => t.Id == BackId
                    ? "[grey]← Back[/]"
                    : $"{Markup.Escape(Format.Truncate(t.Title, 42))} [grey]— {Markup.Escape(Format.Truncate(t.Artist, 24))}[/] [grey]({Format.Duration(t.Duration)})[/]")
                .AddChoices(choices));

        if (picked.Id == BackId)
        {
            return null;
        }

        var startIndex = 0;
        for (var i = 0; i < tracks.Count; i++)
        {
            if (tracks[i].Id == picked.Id)
            {
                startIndex = i;
                break;
            }
        }

        return new PlayRequest(tracks, startIndex);
    }

    private const string BackId = "__back__";
}
