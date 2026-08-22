using Spectre.Console;
using YandexMusic;
using YandexMusic.Player.Auth;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Screens;
using YandexMusic.Player.Ui;

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
    private readonly TrackListScreen _trackList;
    private readonly NowPlayingScreen _nowPlaying;
    private readonly RemoteScreen _remote;
    private readonly NoticeBoard _notices;

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
        TrackListScreen trackList,
        NowPlayingScreen nowPlaying,
        RemoteScreen remote,
        NoticeBoard notices)
    {
        _client = client;
        _auth = auth;
        _controller = controller;
        _catalog = catalog;
        _menu = menu;
        _search = search;
        _albums = albums;
        _playlists = playlists;
        _trackList = trackList;
        _nowPlaying = nowPlaying;
        _remote = remote;
        _notices = notices;
    }

    /// <summary>Runs the app until the user quits.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Write(new FigletText("Yandex Music").Color(Color.Yellow));
        AnsiConsole.MarkupLine($"[grey]{Strings.Subtitle}[/]\n");

        if (!await _auth.EnsureSignedInAsync(_client, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var action = await _menu.RunAsync(cancellationToken).ConfigureAwait(false);
            try
            {
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
                    case MainMenuAction.Liked:
                        await PlayAndShowAsync(await _trackList.RunLikedAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                        break;
                    case MainMenuAction.MyWave:
                        await PlayAndShowAsync(await _trackList.RunWaveAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
                        break;
                    case MainMenuAction.NowPlaying:
                        await _nowPlaying.RunAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case MainMenuAction.Remote:
                        await _remote.RunAsync(cancellationToken).ConfigureAwait(false);
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
            catch (Exception ex)
            {
                // A failing screen (network drop, API error, protocol quirk) must never exit the
                // app: hand the error to the menu, which shows it briefly and lets it expire.
                _notices.Post(Strings.ScreenFailed(ex.Message));
            }
        }
    }

    private async Task PlayAndShowAsync(PlayRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return;
        }

        var origin = request.Origin ?? new PlaybackOrigin("app");
        var items = request.Tracks.Select(t => TrackList.ToPlaybackItem(t, origin, _catalog)).ToList();
        Func<CancellationToken, Task<IReadOnlyList<PlaybackItem>>>? continuation = null;
        if (request.Origin?.Station is { Length: > 0 } station)
        {
            continuation = ct => FetchRadioItemsAsync(station, ct);
        }
        await _controller.PlayAsync(items, request.StartIndex, continuation, cancellationToken).ConfigureAwait(false);
        await _nowPlaying.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<PlaybackItem>> FetchRadioItemsAsync(string station, CancellationToken cancellationToken)
    {
        var batch = await _catalog.GetRadioAsync(station, cancellationToken).ConfigureAwait(false);
        var origin = new PlaybackOrigin("radio", Station: batch.Station, BatchId: batch.BatchId);
        return batch.Tracks.Select(t => TrackList.ToPlaybackItem(t, origin, _catalog)).ToList();
    }
}
