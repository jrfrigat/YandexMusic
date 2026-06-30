using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Lists the user's liked albums and drills into one via the <see cref="AlbumScreen"/>.</summary>
public sealed class AlbumsScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly AlbumScreen _albumScreen;

    /// <summary>Creates the albums screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="albumScreen">The album detail screen to drill into.</param>
    public AlbumsScreen(IMusicCatalog catalog, AlbumScreen albumScreen)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(albumScreen);
        _catalog = catalog;
        _albumScreen = albumScreen;
    }

    /// <summary>Runs the screen.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(CancellationToken cancellationToken = default)
    {
        var albums = await AnsiConsole.Status()
            .StartAsync("Loading your albums…", _ => _catalog.GetMyAlbumsAsync(cancellationToken))
            .ConfigureAwait(false);

        if (albums.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No liked albums (like some albums in the app, and make sure you are signed in).[/]");
            return null;
        }

        var back = new AlbumView(BackId, "← Back", string.Empty, null, 0);
        var choices = new List<AlbumView>(albums) { back };

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<AlbumView>()
                .Title($"Your albums ([green]{albums.Count}[/]):")
                .PageSize(15)
                .MoreChoicesText("[grey](move up/down for more)[/]")
                .UseConverter(a => a.Id == BackId
                    ? "[grey]← Back[/]"
                    : $"{Markup.Escape(Format.Truncate(a.Title, 40))} [grey]— {Markup.Escape(Format.Truncate(a.Artist, 24))}[/]")
                .AddChoices(choices));

        if (picked.Id == BackId)
        {
            return null;
        }

        return await _albumScreen.RunAsync(picked.Id, cancellationToken).ConfigureAwait(false);
    }

    private const string BackId = "__back__";
}
