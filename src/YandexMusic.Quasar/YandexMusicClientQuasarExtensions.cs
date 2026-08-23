namespace YandexMusic.Quasar;

/// <summary>
/// Adds Quasar to an <see cref="IYandexMusicClient"/>. As with Ynison, the method lives here rather
/// than on the client interface so the core package never names a type from this one.
/// </summary>
public static class YandexMusicClientQuasarExtensions
{
    /// <summary>
    /// Creates an <see cref="IQuasarClient"/> for the account signed in to <paramref name="client"/>.
    /// The token is read from the session at call time; the returned client is independent of
    /// <paramref name="client"/> and must be disposed separately.
    /// </summary>
    /// <param name="client">The signed-in client whose session the Quasar requests use.</param>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> to send Quasar requests with, or <see langword="null"/> to create
    /// one. Supply your own to control timeouts, proxying or handler pooling; it is not disposed here.
    /// </param>
    /// <returns>The Quasar client.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The client is not signed in.</exception>
    public static IQuasarClient CreateQuasarClient(this IYandexMusicClient client, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        var token = client.Authentication.Session.AccessToken ?? throw new InvalidOperationException(
            "Sign in before creating a Quasar client; the session has no access token yet.");

        return new QuasarClient(token, httpClient);
    }
}
