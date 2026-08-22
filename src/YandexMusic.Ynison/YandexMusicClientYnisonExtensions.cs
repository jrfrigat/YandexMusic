namespace YandexMusic.Ynison;

/// <summary>
/// Adds Ynison to an <see cref="IYandexMusicClient"/>. The method lives here rather than on the
/// client interface so the core package never names a websocket type: consumers who only need the
/// REST API do not carry this one.
/// </summary>
public static class YandexMusicClientYnisonExtensions
{
    /// <summary>
    /// Creates an <see cref="IYnisonClient"/> for the account signed in to <paramref name="client"/>:
    /// a websocket subscription to the account's playback state across devices and a channel for
    /// remote-control commands. The token and device id are read from the session at call time; the
    /// returned client is independent of <paramref name="client"/> and must be disposed separately,
    /// and it keeps working after the client that created it is gone.
    /// </summary>
    /// <param name="client">The signed-in client whose session the Ynison connection uses.</param>
    /// <param name="deviceId">Overrides the session's device id for the Ynison session.</param>
    /// <param name="options">The Ynison client options, or <see langword="null"/> for defaults.</param>
    /// <returns>The Ynison session, not yet started.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The client is not signed in.</exception>
    public static IYnisonClient CreateYnisonClient(
        this IYandexMusicClient client,
        string? deviceId = null,
        YnisonClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        var session = client.Authentication.Session;
        var token = session.AccessToken ?? throw new InvalidOperationException(
            "Sign in before creating a Ynison client; the session has no access token yet.");
        return new YnisonClient(token, deviceId ?? session.DeviceId, options);
    }
}
