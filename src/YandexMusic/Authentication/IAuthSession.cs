using System.Net;

namespace YandexMusic.Authentication;

/// <summary>
/// Holds the authentication state of a <see cref="YandexMusicClient"/>: the access token, the
/// cookie container and the device identity. The state can be exported to a serializable
/// <see cref="AuthSnapshot"/> and restored later via <see cref="Import"/>.
/// </summary>
public interface IAuthSession
{
    /// <summary>
    /// The OAuth access token currently used to authorize requests, or <see langword="null"/> when
    /// the session is not authenticated.
    /// </summary>
    string? AccessToken { get; }

    /// <summary>
    /// Gets a value indicating whether the session currently holds an access token.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>The device identifier reported to the API for this session.</summary>
    string DeviceId { get; }

    /// <summary>
    /// The cookie container shared with the underlying HTTP handler. Used by the cookie and QR
    /// sign-in flows.
    /// </summary>
    CookieContainer Cookies { get; }

    /// <summary>
    /// Exports the current session state to a serializable snapshot.
    /// </summary>
    /// <returns>A snapshot that can be persisted and later restored with <see cref="Import"/>.</returns>
    AuthSnapshot Export();

    /// <summary>
    /// Restores the session state from a previously exported snapshot, replacing the current token,
    /// device identifier and cookies.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore.</param>
    void Import(AuthSnapshot snapshot);
}
