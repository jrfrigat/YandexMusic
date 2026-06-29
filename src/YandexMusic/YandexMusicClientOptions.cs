using System.Net;

namespace YandexMusic;

/// <summary>
/// Configuration for a <see cref="YandexMusicClient"/>: networking, identity and localization
/// settings applied to every request the client makes.
/// </summary>
public sealed class YandexMusicClientOptions
{
    /// <summary>
    /// The per-request timeout. The default is 30 seconds. Use <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>
    /// to disable the timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The <c>User-Agent</c> header sent with every request. When <see langword="null"/> a default,
    /// browser-like value is used.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// An optional proxy used for all requests. When <see langword="null"/> the system default is used.
    /// </summary>
    public IWebProxy? Proxy { get; set; }

    /// <summary>
    /// The device identifier reported to the Yandex Music API in the <c>X-Yandex-Music-Client</c>
    /// header and related fields. Defaults to <c>"csharp"</c>.
    /// </summary>
    public string DeviceId { get; set; } = "csharp";

    /// <summary>
    /// The language used for localized responses, sent in the <c>Accept-Language</c> header.
    /// Defaults to <c>"ru"</c>.
    /// </summary>
    public string Language { get; set; } = "ru";
}
