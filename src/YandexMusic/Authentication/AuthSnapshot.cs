namespace YandexMusic.Authentication;

/// <summary>
/// A serializable snapshot of an <see cref="IAuthSession"/>. Persist it (for example as JSON in a
/// secure store) to resume a signed-in session later — including a half-finished interactive
/// sign-in — without forcing the user to authenticate again.
/// </summary>
public sealed record AuthSnapshot
{
    /// <summary>The OAuth access token used to authorize Yandex Music API requests, if any.</summary>
    public string? AccessToken { get; init; }

    /// <summary>The device identifier associated with the session.</summary>
    public string DeviceId { get; init; } = "csharp";

    /// <summary>The cookies associated with the session (used by the cookie and QR sign-in flows).</summary>
    public IReadOnlyList<CookieSnapshot> Cookies { get; init; } = [];
}

/// <summary>
/// A single cookie captured in an <see cref="AuthSnapshot"/>.
/// </summary>
public sealed record CookieSnapshot
{
    /// <summary>The cookie name.</summary>
    public required string Name { get; init; }

    /// <summary>The cookie value.</summary>
    public required string Value { get; init; }

    /// <summary>The domain the cookie belongs to.</summary>
    public required string Domain { get; init; }

    /// <summary>The path the cookie applies to. Defaults to <c>"/"</c>.</summary>
    public string Path { get; init; } = "/";
}
