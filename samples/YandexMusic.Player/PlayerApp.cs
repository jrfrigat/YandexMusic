using Spectre.Console;
using YandexMusic;
using YandexMusic.Player.Auth;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Screens;

namespace YandexMusic.Player;

/// <summary>
/// The top-level application: shows the banner, ensures the user is signed in, then runs the main
/// menu, dispatching to the feature screens and the now-playing view. It owns no UI details itself —
/// it just wires screens, auth and playback together.
/// </summary>
public sealed class PlayerApp
{
    private readonly IYandexMusicClient _client;
    private readonly AuthService _auth;
    private readonly PlaybackController _controller;
    private readonly IMusicCatalog _catalog;
    private readonly MainMenuScreen _menu;
    private readonly SearchScreen _search;
    private readonly AlbumsScreen _albums;
    private readonly PlaylistsScreen _playlists;
    private readonly NowPlayingScreen _nowPlaying;

    /// <summary>Creates the application.</summary>
    public PlayerApp(
        IYandexMusicClient client,
        AuthService auth,
        PlaybackController controller,
        IMusicCatalog catalog,
        MainMenuScreen menu,
        SearchScreen search,
        AlbumsScreen albums,
        PlaylistsScreen playlists,
        NowPlayingScreen nowPlaying)
    {
        _client = client;
        _auth = auth;
        _controller = controller;
        _catalog = catalog;
        _menu = menu;
        _search = search;
        _albums = albums;
        _playlists = playlists;
        _nowPlaying = nowPlaying;
    }

    /// <summary>Runs the app until the user quits.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Write(new FigletText("Yandex Music").Color(Color.Yellow));
        AnsiConsole.MarkupLine("[grey]terminal music player[/]\n");

        if (!await _auth.EnsureSignedInAsync(_client, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var action = await _menu.RunAsync(cancellationToken).ConfigureAwait(false);
            switch (action)
            {
                case MainMenuAction.Search:
                    await PlayAndShowAsync(await _search.RunAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                    break;
                case MainMenuAction.Albums:
                    await PlayAndShowAsync(await _albums.RunAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                    break;
                case MainMenuAction.Playlists:
                    await PlayAndShowAsync(await _playlists.RunAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                    break;
                case MainMenuAction.NowPlaying:
                    await _nowPlaying.RunAsync(cancellationToken).ConfigureAwait(false);
                    break;
                case MainMenuAction.SignOut:
                    _auth.SignOut(_client);
                    if (!await _auth.EnsureSignedInAsync(_client, cancellationToken).ConfigureAwait(false))
                    {
                        return;
                    }

                    break;
                case MainMenuAction.Quit:
                    return;
            }
        }
    }

    private async Task PlayAndShowAsync(PlayRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return;
        }

        var items = request.Tracks.Select(ToPlaybackItem).ToList();
        await _controller.PlayAsync(items, request.StartIndex, cancellationToken).ConfigureAwait(false);
        await _nowPlaying.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private PlaybackItem ToPlaybackItem(TrackView track) => new(
        track.Id,
        track.Title,
        track.Artist,
        track.Duration,
        ct => _catalog.ResolveStreamUrlAsync(track.Id, ct));
}
