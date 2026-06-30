using System.Net;

namespace YandexMusic.Authentication;

/// <summary>The default <see cref="IAuthenticationClient"/> implementation.</summary>
internal sealed class AuthenticationClient : IAuthenticationClient
{
    private readonly AuthSession _session;
    private readonly PassportAuthenticator _passport;
    private readonly DeviceAuthenticator _device;

    /// <summary>Initializes a new authentication client over the given session.</summary>
    /// <param name="session">The session to manage.</param>
    public AuthenticationClient(AuthSession session)
    {
        _session = session;
        _passport = new PassportAuthenticator(session);
        _device = new DeviceAuthenticator(session);
    }

    /// <inheritdoc />
    public IAuthSession Session => _session;

    /// <inheritdoc />
    public bool IsAuthenticated => _session.IsAuthenticated;

    /// <inheritdoc />
    public void SignInWithToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("The access token must not be null or whitespace.", nameof(accessToken));
        }

        _session.SetAccessToken(accessToken);
    }

    /// <inheritdoc />
    public Task SignInWithCookiesAsync(IEnumerable<Cookie> cookies, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cookies);
        return _passport.SignInWithCookiesAsync(cookies, cancellationToken);
    }

    /// <inheritdoc />
    public Task<QrSignIn> StartQrSignInAsync(CancellationToken cancellationToken = default)
        => _passport.StartQrSignInAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> TryCompleteQrSignInAsync(QrSignIn qrSignIn, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(qrSignIn);
        return _passport.TryCompleteQrSignInAsync(qrSignIn, cancellationToken);
    }

    /// <inheritdoc />
    public Task SignInWithPasswordAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(login);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _passport.SignInWithPasswordAsync(login, password, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DeviceCode> RequestDeviceCodeAsync(DeviceAuthOptions? options = null, CancellationToken cancellationToken = default)
        => _device.RequestDeviceCodeAsync(options, cancellationToken);

    /// <inheritdoc />
    public Task<OAuthToken?> PollDeviceTokenAsync(string deviceCode, DeviceAuthOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceCode);
        return DeviceAuthenticator.PollDeviceTokenAsync(deviceCode, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OAuthToken> SignInWithDeviceFlowAsync(Action<DeviceCode> onCode, DeviceAuthOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(onCode);
        return _device.SignInWithDeviceFlowAsync(onCode, options, cancellationToken);
    }

    /// <inheritdoc />
    public void SignOut() => _session.ClearAccessToken();
}
