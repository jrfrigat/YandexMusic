using System.Globalization;

namespace YandexMusic.Player.Ui;

/// <summary>
/// All user-facing text, in English and Russian. The language follows the OS UI language: on a
/// Russian-language system <see cref="CultureInfo.CurrentUICulture"/> is <c>ru-*</c>, so the interface
/// is Russian out of the box; everything else falls back to English. Strings carry their own
/// Spectre.Console markup; dynamic values are passed in already escaped.
/// </summary>
public static class Strings
{
    // Russian when the OS UI language is Russian; the YM_PLAYER_LANG env var (ru/en) overrides it.
    private static bool Ru
    {
        get
        {
            var forced = Environment.GetEnvironmentVariable("YM_PLAYER_LANG");
            return !string.IsNullOrWhiteSpace(forced)
                ? forced.StartsWith("ru", StringComparison.OrdinalIgnoreCase)
                : CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";
        }
    }

    // Application shell.
    public static string Subtitle => Ru ? "терминальный музыкальный плеер" : "terminal music player";
    public static string Bye => Ru ? "[grey]Пока.[/]" : "[grey]Bye.[/]";
    public static string NeedsInteractive => Ru
        ? "[yellow]Плееру нужен интерактивный терминал[/] — запустите его напрямую, не через конвейер."
        : "[yellow]This player needs an interactive terminal[/] — run it directly, not through a pipe.";

    // Main menu.
    public static string Menu => Ru ? "[yellow] Меню [/]" : "[yellow] Menu [/]";
    public static string MenuSearch => Ru ? "Поиск треков" : "Search tracks";
    public static string MenuAlbums => Ru ? "Мои альбомы" : "My albums";
    public static string MenuPlaylists => Ru ? "Мои плейлисты" : "My playlists";
    public static string MenuOpenPlayer => Ru ? "Открыть плеер" : "Open player";
    public static string MenuSignOut => Ru ? "Выйти из аккаунта" : "Sign out";
    public static string MenuQuit => Ru ? "Выход" : "Quit";
    public static string NothingPlaying => Ru ? "[grey]♪ ничего не играет[/]" : "[grey]♪ nothing playing[/]";
    public static string MenuHotkeys => Ru
        ? "[grey]↑↓[/] выбор   [grey]enter[/] открыть   [grey]s[/] поиск   [grey]a[/] альбомы   [grey]l[/] плейлисты   [grey]p[/] плеер   [grey]o[/] выход из аккаунта   [grey]q[/] выход"
        : "[grey]↑↓[/] move   [grey]enter[/] select   [grey]s[/] search   [grey]a[/] albums   [grey]l[/] playlists   [grey]p[/] open player   [grey]o[/] sign out   [grey]q[/] quit";

    // Shared list controls.
    public static string Back => Ru ? "← Назад" : "← Back";
    public static string BackDim => Ru ? "[grey]← Назад[/]" : "[grey]← Back[/]";
    public static string MoreChoices => Ru ? "[grey](листайте вверх/вниз)[/]" : "[grey](move up/down for more)[/]";

    // Authentication.
    public static string HowToSignIn => Ru ? "Как вы хотите [yellow]войти[/]?" : "How would you like to [yellow]sign in[/]?";
    public static string AuthQuit => Ru ? "Выход" : "Quit";
    public static string SignedIn => Ru ? "[green]Вход выполнен.[/]" : "[green]Signed in.[/]";
    public static string SignInIncomplete => Ru
        ? "[red]Вход не завершён. Попробуйте другой способ.[/]"
        : "[red]Sign-in did not complete. Try another method.[/]";
    public static string SignInFailed(string message) => Ru ? $"[red]Ошибка входа:[/] {message}" : $"[red]Sign-in failed:[/] {message}";

    public static string FlowToken => Ru ? "OAuth-токен" : "OAuth token";
    public static string FlowQr => Ru ? "QR-код" : "QR code";
    public static string FlowDevice => Ru ? "Код устройства (открыть страницу, ввести код)" : "Device code (open a page, enter a code)";
    public static string FlowPassword => Ru ? "Логин и пароль (по возможности)" : "Login + password (best-effort)";

    public static string PromptToken => Ru ? "Вставьте ваш [yellow]OAuth-токен[/]:" : "Paste your [yellow]OAuth token[/]:";
    public static string ScanWithApp => Ru ? "[grey]Отсканируйте код в приложении Яндекс:[/]" : "[grey]Scan this with the Yandex app:[/]";
    public static string OrOpen(string url) => Ru ? $"[grey]…или откройте:[/] [blue underline]{url}[/]" : $"[grey]…or open:[/] [blue underline]{url}[/]";
    public static string EnterCode(string code) => Ru ? $"…и введите код [yellow]{code}[/]" : $"…and enter code [yellow]{code}[/]";
    public static string WaitingConfirmation => Ru ? "Ожидание подтверждения…" : "Waiting for confirmation…";
    public static string OpenAndEnterCode(string url, string code) => Ru
        ? $"Откройте [blue underline]{url}[/] и введите код [yellow]{code}[/]"
        : $"Open [blue underline]{url}[/] and enter code [yellow]{code}[/]";
    public static string DeviceSignInFailed(string message) => Ru ? $"[red]Ошибка входа по коду устройства:[/] {message}" : $"[red]Device sign-in failed:[/] {message}";
    public static string PromptLogin => Ru ? "[yellow]Логин[/]:" : "[yellow]Login[/]:";
    public static string PromptPassword => Ru ? "[yellow]Пароль[/]:" : "[yellow]Password[/]:";
    public static string PasswordSignInFailed(string message) => Ru ? $"[red]Ошибка входа по паролю:[/] {message}" : $"[red]Password sign-in failed:[/] {message}";
    public static string PasswordHint => Ru
        ? "[grey]Если включена 2FA или капча — используйте вход по QR-коду или коду устройства.[/]"
        : "[grey]If 2FA or a captcha is enabled, use the QR or device-code method.[/]";

    // Search.
    public static string SearchPrompt => Ru ? "[yellow]Поиск треков[/] [grey](пусто — назад)[/]:" : "[yellow]Search tracks[/] [grey](empty to go back)[/]:";
    public static string Searching => Ru ? "Поиск…" : "Searching…";
    public static string NothingFound => Ru ? "[grey]Ничего не найдено.[/]" : "[grey]Nothing found.[/]";
    public static string SearchResultsTitle(int count, string query) => Ru
        ? $"[green]{count}[/] результатов по запросу [yellow]{query}[/]:"
        : $"[green]{count}[/] results for [yellow]{query}[/]:";

    // Albums.
    public static string LoadingAlbums => Ru ? "Загрузка ваших альбомов…" : "Loading your albums…";
    public static string NoAlbums => Ru
        ? "[grey]Нет понравившихся альбомов (отметьте альбомы лайком и войдите в аккаунт).[/]"
        : "[grey]No liked albums (like some albums in the app, and make sure you are signed in).[/]";
    public static string YourAlbums(int count) => Ru ? $"Ваши альбомы ([green]{count}[/]):" : $"Your albums ([green]{count}[/]):";
    public static string LoadingAlbum => Ru ? "Загрузка альбома…" : "Loading album…";
    public static string AlbumNoTracks => Ru ? "[grey]В альбоме нет доступных треков.[/]" : "[grey]The album has no playable tracks.[/]";
    public static string PlayFromWhich => Ru ? "С какого трека начать?" : "Play from which track?";

    // Playlists.
    public static string LoadingPlaylists => Ru ? "Загрузка ваших плейлистов…" : "Loading your playlists…";
    public static string NoPlaylists => Ru ? "[grey]Плейлисты не найдены (войдите в аккаунт).[/]" : "[grey]No playlists found (make sure you are signed in).[/]";
    public static string YourPlaylists(int count) => Ru ? $"Ваши плейлисты ([green]{count}[/]):" : $"Your playlists ([green]{count}[/]):";
    public static string TracksSuffix(int count) => Ru ? $"{count} треков" : $"{count} tracks";
    public static string LoadingPlaylist => Ru ? "Загрузка плейлиста…" : "Loading playlist…";
    public static string PlaylistNoTracks => Ru ? "[grey]В плейлисте нет доступных треков.[/]" : "[grey]The playlist has no playable tracks.[/]";

    // Table column headers.
    public static string ColumnTitle => Ru ? "Название" : "Title";
    public static string ColumnArtist => Ru ? "Исполнитель" : "Artist";
    public static string ColumnTime => Ru ? "[grey]Время[/]" : "[grey]Time[/]";

    // Now playing.
    public static string NowPlayingHeader => Ru ? "[green] ♪ Сейчас играет [/]" : "[green] ♪ Now Playing [/]";
    public static string NothingPlayingYet => Ru ? "[grey]Сейчас ничего не играет.[/]" : "[grey]Nothing is playing yet.[/]";
    public static string StatePlaying => Ru ? "[green]▶ Играет[/]" : "[green]▶ Playing[/]";
    public static string StatePaused => Ru ? "[yellow]⏸ Пауза[/]" : "[yellow]⏸ Paused[/]";
    public static string StateBuffering => Ru ? "[blue]… Буферизация[/]" : "[blue]… Buffering[/]";
    public static string StateStopped => Ru ? "[grey]⏹ Остановлено[/]" : "[grey]⏹ Stopped[/]";
    public static string StateEnded => Ru ? "[grey]■ Завершено[/]" : "[grey]■ Ended[/]";
    public static string StateError => Ru ? "[red]! Ошибка[/]" : "[red]! Error[/]";
    public static string StateIdle => Ru ? "[grey]Ожидание[/]" : "[grey]Idle[/]";
    public static string VolumeLabel => Ru ? "громк." : "vol";
    public static string SimulatedSuffix => Ru ? "   ·   симуляция (без звука)" : "   ·   simulated playback (no audio output)";
    public static string TrackCounter(int position, int length) => Ru ? $"Трек {position}/{length}" : $"Track {position}/{length}";
    public static string NowPlayingKeys => Ru
        ? "[grey]space[/] пауза   [grey]←/→[/] пред/след   [grey]↑/↓[/] громкость   [grey]s[/] стоп   [grey]q[/] назад"
        : "[grey]space[/] play/pause   [grey]←/→[/] prev/next   [grey]↑/↓[/] volume   [grey]s[/] stop   [grey]q[/] back";
}
