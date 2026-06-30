using YandexMusic;

namespace YandexMusic.Player.Auth;

/// <summary>
/// One way of signing the client in (token, QR, device-code, password). Each flow owns its own
/// interaction; the auth screen just lists them and runs the chosen one. New methods plug in by
/// adding another implementation — nothing else changes.
/// </summary>
public interface IAuthFlow
{
    /// <summary>The menu label for this method.</summary>
    string Name { get; }

    /// <summary>Runs the flow, signing <paramref name="client"/> in.</summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="cancellationToken">A token to cancel the flow.</param>
    /// <returns><see langword="true"/> when the client became authenticated.</returns>
    Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default);
}

/// <summary>Shared helpers for the auth flows.</summary>
internal static class AuthSupport
{
    /// <summary>Confirms a sign-in actually works by fetching the account status.</summary>
    /// <param name="client">The client to check.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the account could be retrieved.</returns>
    public static async Task<bool> ValidateAsync(IYandexMusicClient client, CancellationToken cancellationToken)
    {
        try
        {
            var status = await client.Account.GetStatusAsync(cancellationToken).ConfigureAwait(false);
            return status is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
