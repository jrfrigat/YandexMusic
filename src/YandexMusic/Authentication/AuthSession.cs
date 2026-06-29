using System.Net;

namespace YandexMusic.Authentication;

/// <summary>
/// The default in-memory implementation of <see cref="IAuthSession"/>. A single instance is owned
/// by each <see cref="YandexMusicClient"/> and shared with its HTTP handler so that cookies set by
/// the server are retained across requests.
/// </summary>
public sealed class AuthSession : IAuthSession
{
    private string? _accessToken;

    /// <summary>Initializes a new, unauthenticated session with a default device identifier.</summary>
    public AuthSession()
    {
    }

    /// <summary>Initializes a new, unauthenticated session with the specified device identifier.</summary>
    /// <param name="deviceId">The device identifier to report to the API.</param>
    public AuthSession(string deviceId)
    {
        DeviceId = deviceId;
    }

    /// <inheritdoc />
    public string? AccessToken => _accessToken;

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);

    /// <inheritdoc />
    public string DeviceId { get; private set; } = "csharp";

    /// <inheritdoc />
    public CookieContainer Cookies { get; } = new();

    /// <inheritdoc />
    public AuthSnapshot Export()
    {
        var cookies = Cookies.GetAllCookies();
        var snapshot = new CookieSnapshot[cookies.Count];
        for (var i = 0; i < cookies.Count; i++)
        {
            var cookie = cookies[i];
            snapshot[i] = new CookieSnapshot
            {
                Name = cookie.Name,
                Value = cookie.Value,
                Domain = cookie.Domain,
                Path = cookie.Path,
            };
        }

        return new AuthSnapshot
        {
            AccessToken = _accessToken,
            DeviceId = DeviceId,
            Cookies = snapshot,
        };
    }

    /// <inheritdoc />
    public void Import(AuthSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _accessToken = snapshot.AccessToken;
        DeviceId = snapshot.DeviceId;

        foreach (var cookie in snapshot.Cookies)
        {
            Cookies.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
        }
    }

    /// <summary>Sets the access token used to authorize requests.</summary>
    /// <param name="accessToken">The OAuth access token.</param>
    internal void SetAccessToken(string accessToken) => _accessToken = accessToken;

    /// <summary>Sets the device identifier reported to the API.</summary>
    /// <param name="deviceId">The device identifier.</param>
    internal void SetDeviceId(string deviceId) => DeviceId = deviceId;

    /// <summary>Clears the access token, returning the session to an unauthenticated state.</summary>
    internal void ClearAccessToken() => _accessToken = null;
}
