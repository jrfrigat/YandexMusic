using Spectre.Console;
using YandexMusicTerminal.Catalog;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>Lists the user's own playlists and drills into one via the <see cref="PlaylistScreen"/>.</summary>
public sealed class PlaylistsScreen
{
    private readonly IMusicCatalog _catalog;
    private readonly PlaylistScreen _playlistScreen;
    private readonly NoticeBoard _notices;

    /// <summary>Creates the playlists screen.</summary>
    /// <param name="catalog">The catalog to query.</param>
    /// <param name="playlistScreen">The playlist detail screen to drill into.</param>
    /// <param name="notices">The board an empty library reports to.</param>
    public PlaylistsScreen(IMusicCatalog catalog, PlaylistScreen playlistScreen, NoticeBoard notices)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(playlistScreen);
        ArgumentNullException.ThrowIfNull(notices);
        _catalog = catalog;
        _playlistScreen = playlistScreen;
        _notices = notices;
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
            _notices.Post(Strings.NoPlaylists);
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
