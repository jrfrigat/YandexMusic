using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Shows an album's tracklist and lets the user start playing from any track.</summary>
public sealed class AlbumScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the album screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public AlbumScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <summary>Runs the screen for a given album.</summary>
    /// <param name="albumId">The album to show.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(string albumId, CancellationToken cancellationToken = default)
    {
        var detail = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingAlbum, _ => _catalog.GetAlbumAsync(albumId, cancellationToken))
            .ConfigureAwait(false);

        if (detail is null || detail.Tracks.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.AlbumNoTracks);
            return null;
        }

        var album = detail.Album;
        var year = album.Year?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "—";
        AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(album.Title)}[/] [grey]— {Markup.Escape(album.Artist)} · {year}[/]").LeftJustified());

        var table = new Table().Border(TableBorder.Minimal).BorderColor(Color.Grey);
        table.AddColumn("[grey]#[/]");
        table.AddColumn(Strings.ColumnTitle);
        table.AddColumn(Strings.ColumnTime);
        var number = 1;
        foreach (var track in detail.Tracks)
        {
            table.AddRow(
                $"[grey]{number++}[/]",
                Markup.Escape(track.Title),
                $"[grey]{Format.Duration(track.Duration)}[/]");
        }

        AnsiConsole.Write(table);

        var back = new TrackView(BackId, Strings.Back, string.Empty, null, TimeSpan.Zero);
        var choices = new List<TrackView>(detail.Tracks) { back };

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<TrackView>()
                .Title(Strings.PlayFromWhich)
                .PageSize(15)
                .UseConverter(t => t.Id == BackId ? Strings.BackDim : Markup.Escape(Format.Truncate(t.Title, 50)))
                .AddChoices(choices));

        if (picked.Id == BackId)
        {
            return null;
        }

        var startIndex = 0;
        for (var i = 0; i < detail.Tracks.Count; i++)
        {
            if (detail.Tracks[i].Id == picked.Id)
            {
                startIndex = i;
                break;
            }
        }

        return new PlayRequest(detail.Tracks, startIndex);
    }

    private const string BackId = "__back__";
}
