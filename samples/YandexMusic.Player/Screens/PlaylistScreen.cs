using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Shows a playlist's tracklist and lets the user start playing from any track.</summary>
public sealed class PlaylistScreen
{
    private readonly IMusicCatalog _catalog;

    /// <summary>Creates the playlist screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    public PlaylistScreen(IMusicCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        _catalog = catalog;
    }

    /// <summary>Runs the screen for a given playlist.</summary>
    /// <param name="playlistId">The playlist to show.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var detail = await AnsiConsole.Status()
            .StartAsync("Loading playlist…", _ => _catalog.GetPlaylistAsync(playlistId, cancellationToken))
            .ConfigureAwait(false);

        if (detail is null || detail.Tracks.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]The playlist has no playable tracks.[/]");
            return null;
        }

        var playlist = detail.Playlist;
        AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(playlist.Title)}[/] [grey]· {playlist.TrackCount} tracks[/]").LeftJustified());

        var table = new Table().Border(TableBorder.Minimal).BorderColor(Color.Grey);
        table.AddColumn("[grey]#[/]");
        table.AddColumn("Title");
        table.AddColumn("Artist");
        table.AddColumn("[grey]Time[/]");
        var number = 1;
        foreach (var track in detail.Tracks)
        {
            table.AddRow(
                $"[grey]{number++}[/]",
                Markup.Escape(Format.Truncate(track.Title, 40)),
                $"[grey]{Markup.Escape(Format.Truncate(track.Artist, 28))}[/]",
                $"[grey]{Format.Duration(track.Duration)}[/]");
        }

        AnsiConsole.Write(table);

        var back = new TrackView(BackId, "← Back", string.Empty, null, TimeSpan.Zero);
        var choices = new List<TrackView>(detail.Tracks) { back };

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<TrackView>()
                .Title("Play from which track?")
                .PageSize(15)
                .MoreChoicesText("[grey](move up/down for more)[/]")
                .UseConverter(t => t.Id == BackId
                    ? "[grey]← Back[/]"
                    : $"{Markup.Escape(Format.Truncate(t.Title, 42))} [grey]— {Markup.Escape(Format.Truncate(t.Artist, 24))}[/]")
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
