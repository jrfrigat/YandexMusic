using System.Globalization;
using System.Resources;

namespace YandexMusic.Player.Ui;

/// <summary>
/// Strongly-typed access to the player's localized strings. The text itself lives in the
/// <c>Resources/Strings.resx</c> (English, neutral) and <c>Strings.ru.resx</c> (Russian) resource
/// files; the .NET SDK embeds them and builds a satellite assembly per culture. The right language is
/// chosen by <see cref="CultureInfo.CurrentUICulture"/> (the OS UI language), which
/// <see cref="Program"/> may override from the <c>YM_PLAYER_LANG</c> environment variable.
/// </summary>
public static class Strings
{
    private static readonly ResourceManager Manager = new("YandexMusic.Player.Resources.Strings", typeof(Strings).Assembly);

    private static string Get(string key) => Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    private static string Fmt(string key, params object[] args) => string.Format(CultureInfo.CurrentCulture, Get(key), args);

    // Application shell.
    public static string Subtitle => Get(nameof(Subtitle));
    public static string Bye => Get(nameof(Bye));
    public static string NeedsInteractive => Get(nameof(NeedsInteractive));

    // Main menu.
    public static string Menu => Get(nameof(Menu));
    public static string MenuSearch => Get(nameof(MenuSearch));
    public static string MenuAlbums => Get(nameof(MenuAlbums));
    public static string MenuPlaylists => Get(nameof(MenuPlaylists));
    public static string MenuOpenPlayer => Get(nameof(MenuOpenPlayer));
    public static string MenuSignOut => Get(nameof(MenuSignOut));
    public static string MenuQuit => Get(nameof(MenuQuit));
    public static string NothingPlaying => Get(nameof(NothingPlaying));
    public static string MenuHotkeys => Get(nameof(MenuHotkeys));

    // Shared list controls.
    public static string Back => Get(nameof(Back));
    public static string BackDim => Get(nameof(BackDim));
    public static string MoreChoices => Get(nameof(MoreChoices));

    // Authentication.
    public static string HowToSignIn => Get(nameof(HowToSignIn));
    public static string AuthQuit => Get(nameof(AuthQuit));
    public static string SignedIn => Get(nameof(SignedIn));
    public static string SignInIncomplete => Get(nameof(SignInIncomplete));
    public static string SignInFailed(string message) => Fmt(nameof(SignInFailed), message);

    public static string FlowToken => Get(nameof(FlowToken));
    public static string FlowQr => Get(nameof(FlowQr));
    public static string FlowDevice => Get(nameof(FlowDevice));
    public static string FlowPassword => Get(nameof(FlowPassword));

    public static string PromptToken => Get(nameof(PromptToken));
    public static string ScanWithApp => Get(nameof(ScanWithApp));
    public static string OrOpen(string url) => Fmt(nameof(OrOpen), url);
    public static string EnterCode(string code) => Fmt(nameof(EnterCode), code);
    public static string WaitingConfirmation => Get(nameof(WaitingConfirmation));
    public static string OpenAndEnterCode(string url, string code) => Fmt(nameof(OpenAndEnterCode), url, code);
    public static string DeviceSignInFailed(string message) => Fmt(nameof(DeviceSignInFailed), message);
    public static string PromptLogin => Get(nameof(PromptLogin));
    public static string PromptPassword => Get(nameof(PromptPassword));
    public static string PasswordSignInFailed(string message) => Fmt(nameof(PasswordSignInFailed), message);
    public static string PasswordHint => Get(nameof(PasswordHint));

    // Search.
    public static string SearchPrompt => Get(nameof(SearchPrompt));
    public static string Searching => Get(nameof(Searching));
    public static string NothingFound => Get(nameof(NothingFound));
    public static string SearchResultsTitle(int count, string query) => Fmt(nameof(SearchResultsTitle), count, query);

    // Albums.
    public static string LoadingAlbums => Get(nameof(LoadingAlbums));
    public static string NoAlbums => Get(nameof(NoAlbums));
    public static string YourAlbums(int count) => Fmt(nameof(YourAlbums), count);
    public static string LoadingAlbum => Get(nameof(LoadingAlbum));
    public static string AlbumNoTracks => Get(nameof(AlbumNoTracks));
    public static string PlayFromWhich => Get(nameof(PlayFromWhich));

    // Playlists.
    public static string LoadingPlaylists => Get(nameof(LoadingPlaylists));
    public static string NoPlaylists => Get(nameof(NoPlaylists));
    public static string YourPlaylists(int count) => Fmt(nameof(YourPlaylists), count);
    public static string TracksSuffix(int count) => Fmt(nameof(TracksSuffix), count);
    public static string LoadingPlaylist => Get(nameof(LoadingPlaylist));
    public static string PlaylistNoTracks => Get(nameof(PlaylistNoTracks));

    // Table column headers.
    public static string ColumnTitle => Get(nameof(ColumnTitle));
    public static string ColumnArtist => Get(nameof(ColumnArtist));
    public static string ColumnTime => Get(nameof(ColumnTime));

    // Now playing.
    public static string NowPlayingHeader => Get(nameof(NowPlayingHeader));
    public static string NothingPlayingYet => Get(nameof(NothingPlayingYet));
    public static string StatePlaying => Get(nameof(StatePlaying));
    public static string StatePaused => Get(nameof(StatePaused));
    public static string StateBuffering => Get(nameof(StateBuffering));
    public static string StateStopped => Get(nameof(StateStopped));
    public static string StateEnded => Get(nameof(StateEnded));
    public static string StateError => Get(nameof(StateError));
    public static string StateIdle => Get(nameof(StateIdle));
    public static string VolumeLabel => Get(nameof(VolumeLabel));
    public static string SimulatedSuffix => Get(nameof(SimulatedSuffix));
    public static string TrackCounter(int position, int length) => Fmt(nameof(TrackCounter), position, length);
    public static string NowPlayingKeys => Get(nameof(NowPlayingKeys));
}
