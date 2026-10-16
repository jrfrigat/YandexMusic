namespace YandexMusic.Ynison;

/// <summary>Options for <see cref="YnisonClient"/>.</summary>
public sealed record YnisonClientOptions
{
    /// <summary>The default Ynison endpoint.</summary>
    public const string DefaultBaseUri = "wss://ynison.music.yandex.ru";

    /// <summary>The Ynison endpoint; host-only (no path). Defaults to the official service.</summary>
    public string BaseUri { get; init; } = DefaultBaseUri;

    /// <summary>
    /// The application name reported to other participants of the session. Defaults to
    /// "YandexMusic .NET".
    /// </summary>
    public string AppName { get; init; } = "YandexMusic .NET";

    /// <summary>
    /// The client ping interval for the state socket. When <see langword="null"/>, the interval
    /// recommended by the redirector is used, falling back to 20 seconds.
    /// </summary>
    public TimeSpan? KeepAliveInterval { get; init; }

    /// <summary>
    /// How many reconnects in a row may fail before <see cref="YnisonClient.RunAsync"/> gives up with
    /// the last failure. <see langword="null"/> (the default) retries without limit. A connection
    /// that delivers state resets the count.
    /// </summary>
    public int? MaxReconnectAttempts { get; init; }
}
