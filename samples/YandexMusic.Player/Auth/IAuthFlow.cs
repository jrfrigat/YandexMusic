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

/// <summary>Outcome of checking a restored session against the live account status.</summary>
public enum SessionCheck
{
    /// <summary>The account status was fetched; the session works.</summary>
    Valid,

    /// <summary>The API refused the session; it is stale.</summary>
    Rejected,

    /// <summary>The API could not be reached; nothing is known about the session.</summary>
    Unreachable,
}

/// <summary>Shared helpers for the auth flows.</summary>
internal static class AuthSupport
{
    /// <summary>Confirms a sign-in actually works by fetching the account status.</summary>
    /// <param name="client">The client to check.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>Whether the session is <see cref="SessionCheck.Valid"/>, was <see cref="SessionCheck.Rejected"/>
    /// by the API, or the API is <see cref="SessionCheck.Unreachable"/>.</returns>
    public static async Task<SessionCheck> ValidateAsync(IYandexMusicClient client, CancellationToken cancellationToken)
    {
        try
        {
            var status = await client.Account.GetStatusAsync(cancellationToken).ConfigureAwait(false);
            return status is not null ? SessionCheck.Valid : SessionCheck.Rejected;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Ctrl+C: the app is exiting; keep the session and let the cancellation propagate.
        }
        catch (OperationCanceledException)
        {
            return SessionCheck.Unreachable; // A timeout, not a user cancellation.
        }
        catch (YandexMusicException)
        {
            return SessionCheck.Rejected; // The API answered and refused the session.
        }
        catch (HttpRequestException)
        {
            return SessionCheck.Unreachable; // A network failure says nothing about the session.
        }
    }
}
