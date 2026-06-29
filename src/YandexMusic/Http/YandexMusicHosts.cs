namespace YandexMusic.Http;

/// <summary>The base URLs of the Yandex services the client talks to.</summary>
internal static class YandexMusicHosts
{
    /// <summary>The main Yandex Music API host.</summary>
    public const string Api = "https://api.music.yandex.net";

    /// <summary>The Yandex Passport host, used by the interactive sign-in flows.</summary>
    public const string Passport = "https://passport.yandex.ru";

    /// <summary>The Yandex OAuth host, used to exchange credentials for tokens.</summary>
    public const string OAuth = "https://oauth.yandex.ru";
}

/// <summary>The names of the custom HTTP headers the Yandex Music API expects.</summary>
internal static class YandexMusicHeaders
{
    /// <summary>Carries the OAuth access token (<c>Authorization: OAuth &lt;token&gt;</c>).</summary>
    public const string Authorization = "Authorization";

    /// <summary>Identifies the client device.</summary>
    public const string Client = "X-Yandex-Music-Client";
}
