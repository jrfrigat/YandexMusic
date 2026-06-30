using Spectre.Console;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Screens;

/// <summary>Lists the user's own playlists and drills into one via the <see cref="PlaylistScreen"/>.</summary>
public sealed class PlaylistsScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly PlaylistScreen _playlistScreen;

    /// <summary>Creates the playlists screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="playlistScreen">The playlist detail screen to drill into.</param>
    public PlaylistsScreen(IMusicCatalog catalog, PlaylistScreen playlistScreen)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(playlistScreen);
        _catalog = catalog;
        _playlistScreen = playlistScreen;
    }

    /// <summary>Runs the screen.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A play request, or <see langword="null"/> to go back.</returns>
    public async Task<PlayRequest?> RunAsync(CancellationToken cancellationToken = default)
    {
        var playlists = await AnsiConsole.Status()
            .StartAsync(Strings.LoadingPlaylists, _ => _catalog.GetMyPlaylistsAsync(cancellationToken))
            .ConfigureAwait(false);

        if (playlists.Count == 0)
        {
            AnsiConsole.MarkupLine(Strings.NoPlaylists);
            return null;
        }

        var picked = await new SelectionView<PlaylistView>(Strings.YourPlaylists(playlists.Count), playlists, Convert)
            .ShowAsync(cancellationToken)
            .ConfigureAwait(false);

        return picked is null ? null : await _playlistScreen.RunAsync(picked.Id, cancellationToken).ConfigureAwait(false);
    }

    private static string Convert(PlaylistView playlist)
        => $"{Markup.Escape(Format.Truncate(playlist.Title, 40))} [grey]· {Strings.TracksSuffix(playlist.TrackCount)}[/]";
}
