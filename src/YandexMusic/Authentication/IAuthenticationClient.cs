using System.Net;

namespace YandexMusic.Authentication;

/// <summary>
/// Manages the authentication of a <see cref="YandexMusicClient"/>. Exposes the current
/// <see cref="Session"/> and the operations used to sign in and out, including the token, cookie and
/// interactive QR flows.
/// </summary>
public interface IAuthenticationClient
{
    /// <summary>The authentication state backing the client.</summary>
    IAuthSession Session { get; }

    /// <summary>Gets a value indicating whether the client currently holds an access token.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Signs in with an existing OAuth access token. The token is stored and sent with every
    /// subsequent request; no network call is made.
    /// </summary>
    /// <param name="accessToken">A valid Yandex Music OAuth access token.</param>
    /// <exception cref="ArgumentException"><paramref name="accessToken"/> is <see langword="null"/> or whitespace.</exception>
    void SignInWithToken(string accessToken);

    /// <summary>
    /// Adds Yandex session cookies and exchanges them for an access token, signing the client in.
    /// </summary>
    /// <param name="cookies">Yandex session cookies for the <c>.yandex.ru</c> domain.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <exception cref="YandexMusicAuthenticationException">A token could not be obtained from the cookies.</exception>
    /// <remarks>
    /// Best-effort implementation of a version-sensitive protocol; verify against the live flow.
    /// </remarks>
    Task SignInWithCookiesAsync(IEnumerable<Cookie> cookies, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts an interactive QR sign-in. Render the returned URL as a QR code, then poll
    /// <see cref="TryCompleteQrSignInAsync"/> until it returns <see langword="true"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The in-progress sign-in.</returns>
    /// <remarks>
    /// Best-effort implementation of a version-sensitive Yandex Passport protocol; verify against the live flow.
    /// </remarks>
    Task<QrSignIn> StartQrSignInAsync(CancellationToken cancellationToken = default);

    /// <summary>Polls a QR sign-in once; on success the client becomes authenticated.</summary>
    /// <param name="qrSignIn">The in-progress sign-in from <see cref="StartQrSignInAsync"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the user has confirmed and a token was obtained.</returns>
    Task<bool> TryCompleteQrSignInAsync(QrSignIn qrSignIn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs in with a login and password via the Yandex Passport flow, then signs the client in.
    /// </summary>
    /// <param name="login">The account login (email, phone or username).</param>
    /// <param name="password">The account password.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <exception cref="ArgumentException"><paramref name="login"/> or <paramref name="password"/> is null or whitespace.</exception>
    /// <exception cref="YandexMusicAuthenticationException">The credentials were rejected or a captcha/2FA is required.</exception>
    /// <remarks>
    /// Best-effort implementation of a version-sensitive, captcha/2FA-gated protocol; verify against the
    /// live flow. The token and device-code paths are the robust, fully-supported options.
    /// </remarks>
    Task SignInWithPasswordAsync(string login, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the OAuth device-code flow by requesting a device code. Show the returned
    /// <see cref="DeviceCode.UserCode"/> and <see cref="DeviceCode.VerificationUrl"/> to the user,
    /// then poll <see cref="PollDeviceTokenAsync"/> with <see cref="DeviceCode.Code"/>.
    /// </summary>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The device code, user code and verification URL.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The service did not return a device code.</exception>
    Task<DeviceCode> RequestDeviceCodeAsync(DeviceAuthOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls the OAuth service once for an access token using a device code obtained from
    /// <see cref="RequestDeviceCodeAsync"/>. On success the client becomes authenticated.
    /// </summary>
    /// <param name="deviceCode">The <see cref="DeviceCode.Code"/> to poll with.</param>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The access token once the user confirms, or <see langword="null"/> while still pending.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The flow failed (for example the code expired or was denied).</exception>
    Task<OAuthToken?> PollDeviceTokenAsync(string deviceCode, DeviceAuthOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the full OAuth device-code flow: requests a code, invokes <paramref name="onCode"/> so the
    /// user code and URL can be shown, then polls until the user confirms, the code expires, or the
    /// operation is cancelled. On success the client is signed in.
    /// </summary>
    /// <param name="onCode">A callback that receives the device code so it can be presented to the user.</param>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The obtained access token.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The user did not confirm before the code expired.</exception>
    Task<OAuthToken> SignInWithDeviceFlowAsync(Action<DeviceCode> onCode, DeviceAuthOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Signs out by discarding the current access token.</summary>
    void SignOut();
}
