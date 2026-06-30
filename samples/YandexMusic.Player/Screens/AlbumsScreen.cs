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
            .StartAsync(Strings.LoadingAlbums, _ => _catalog.GetMyAlbumsAsync(cancellationToken))
            .ConfigureAwait(false);

        if (albums.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.NoAlbums);
            return null;
        }

        var picked = await new SelectionView<AlbumView>(Strings.YourAlbums(albums.Count), albums, Convert)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : await _albumScreen.RunAsync(picked.Id, cancellationToken).ConfigureAwait(false);
    }

    private static string Convert(AlbumView album)
        => $"{Markup.Escape(Format.Truncate(album.Title, 40))} [grey]— {Markup.Escape(Format.Truncate(album.Artist, 24))}[/]";
}
