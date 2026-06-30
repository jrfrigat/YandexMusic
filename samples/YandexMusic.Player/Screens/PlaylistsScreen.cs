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
            .StartAsync("Loading your playlists…", _ => _catalog.GetMyPlaylistsAsync(cancellationToken))
            .ConfigureAwait(false);

        if (playlists.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No playlists found (make sure you are signed in).[/]");
            return null;
        }

        var back = new PlaylistView(BackId, "← Back", 0);
        var choices = new List<PlaylistView>(playlists) { back };

        var picked = AnsiConsole.Prompt(
            new SelectionPrompt<PlaylistView>()
                .Title($"Your playlists ([green]{playlists.Count}[/]):")
                .PageSize(15)
                .MoreChoicesText("[grey](move up/down for more)[/]")
                .UseConverter(p => p.Id == BackId
                    ? "[grey]← Back[/]"
                    : $"{Markup.Escape(Format.Truncate(p.Title, 40))} [grey]· {p.TrackCount} tracks[/]")
                .AddChoices(choices));

        if (picked.Id == BackId)
        {
            return null;
        }

        return await _playlistScreen.RunAsync(picked.Id, cancellationToken).ConfigureAwait(false);
    }

    private const string BackId = "__back__";
}
