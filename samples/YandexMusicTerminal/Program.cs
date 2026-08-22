// Composition root for the terminal player. Everything is wired here and resolved through DI so the
// pieces (catalog, auth flows, audio backend, screens) stay decoupled and individually replaceable.
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using YandexMusic;
using YandexMusicTerminal;
using YandexMusicTerminal.Auth;
using YandexMusicTerminal.Catalog;
using YandexMusicTerminal.Diagnostics;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Screens;
using YandexMusicTerminal.Ui;

// The UI language follows the OS UI language by default; YM_PLAYER_LANG (e.g. "ru" or "en") overrides
// it. Localized strings are resolved from the .resx satellites via CurrentUICulture.
var forcedLanguage = Environment.GetEnvironmentVariable("YM_PLAYER_LANG");
if (!string.IsNullOrWhiteSpace(forcedLanguage))
{
    var culture = CultureInfo.GetCultureInfo(forcedLanguage);
    CultureInfo.CurrentUICulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var services = new ServiceCollection();

// The library client and the app's services. The journal is created first: the client's handler
// pipeline logs through it, and it must exist before anything issues a request.
services.AddSingleton<RequestLog>();
services.AddSingleton<IYandexMusicClient>(provider =>
{
    var log = provider.GetRequiredService<RequestLog>();
    return new YandexMusicClient(new YandexMusicClientOptions { HandlerFactory = () => new LoggingHttpHandler(log) });
});
services.AddSingleton<ISessionStore, FileSessionStore>();
services.AddSingleton<IMusicCatalog, MusicCatalog>();

// Sign-in methods (order = menu order).
services.AddSingleton<IAuthFlow, TokenAuthFlow>();
services.AddSingleton<IAuthFlow, DeviceCodeAuthFlow>();
services.AddSingleton<IAuthFlow, QrAuthFlow>();
services.AddSingleton<IAuthFlow, PasswordAuthFlow>();
services.AddSingleton<AuthService>();

// Audio: a real backend on Windows, the simulated one everywhere (and as a fallback). Swapping in a
// cross-platform backend later means changing only this line.
services.AddSingleton<IAudioPlayer>(_ =>
{
    IAudioPlayer? real = null;
#if WINDOWS
    if (OperatingSystem.IsWindows())
    {
        real = new NAudioPlayer();
    }
#endif
    return new ResilientAudioPlayer(real, new SimulatedAudioPlayer());
});
services.AddSingleton<PlaybackController>();
services.AddSingleton<PlayReporter>();

// Screens.
services.AddSingleton<NoticeBoard>();
services.AddSingleton<UpdateChecker>();
services.AddSingleton<MainMenuScreen>();
services.AddSingleton<SearchScreen>();
services.AddSingleton<AlbumScreen>();
services.AddSingleton<ArtistScreen>();
services.AddSingleton<AlbumsScreen>();
services.AddSingleton<PlaylistScreen>();
services.AddSingleton<PlaylistsScreen>();
services.AddSingleton<TrackListScreen>();
services.AddSingleton<NowPlayingScreen>();
services.AddSingleton<LyricsScreen>();
services.AddSingleton<RemoteScreen>();
services.AddSingleton<PlayerApp>();

await using var provider = services.BuildServiceProvider();

// The reporter only observes playback, so nothing injects it — pull it once to activate.
_ = provider.GetRequiredService<PlayReporter>();

// Ask GitHub about a newer release, at most once a day and never on the startup path.
provider.GetRequiredService<UpdateChecker>().StartCheck();

if (!AnsiConsole.Profile.Capabilities.Interactive)
{
    AnsiConsole.MarkupLine(Strings.NeedsInteractive);
    return;
}

try
{
    await provider.GetRequiredService<PlayerApp>().RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    // Ctrl+C — exit quietly.
}

AnsiConsole.MarkupLine(Strings.Bye);
