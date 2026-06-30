// Composition root for the terminal player. Everything is wired here and resolved through DI so the
// pieces (catalog, auth flows, audio backend, screens) stay decoupled and individually replaceable.
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using YandexMusic;
using YandexMusic.Player;
using YandexMusic.Player.Auth;
using YandexMusic.Player.Catalog;
using YandexMusic.Player.Playback;
using YandexMusic.Player.Screens;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var services = new ServiceCollection();

// The library client and the app's services.
services.AddSingleton<IYandexMusicClient>(_ => new YandexMusicClient());
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

// Screens.
services.AddSingleton<MainMenuScreen>();
services.AddSingleton<SearchScreen>();
services.AddSingleton<AlbumScreen>();
services.AddSingleton<AlbumsScreen>();
services.AddSingleton<PlaylistScreen>();
services.AddSingleton<PlaylistsScreen>();
services.AddSingleton<NowPlayingScreen>();
services.AddSingleton<PlayerApp>();

await using var provider = services.BuildServiceProvider();

if (!AnsiConsole.Profile.Capabilities.Interactive)
{
    AnsiConsole.MarkupLine("[yellow]This player needs an interactive terminal[/] — run it directly, not through a pipe.");
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

AnsiConsole.MarkupLine("[grey]Bye.[/]");
