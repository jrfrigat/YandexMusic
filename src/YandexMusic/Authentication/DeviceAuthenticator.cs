using System.Net;
using System.Text.Json;
using YandexMusic.Serialization;

namespace YandexMusic.Authentication;

/// <summary>
/// Implements the OAuth device-code sign-in flow against the Yandex identity service. This is the
/// official, documented way to obtain an access token interactively without handling the user's
/// password: the user confirms a short code on a Yandex page and the library polls for the token.
/// </summary>
internal sealed class DeviceAuthenticator
{
    private const string DeviceCodeUrl = "https://oauth.yandex.ru/device/code";
    private const string TokenUrl = "https://oauth.yandex.ru/token";

    // OAuth signals that the user has not yet confirmed the code; the caller should keep polling.
    private const string AuthorizationPending = "authorization_pending";

    private readonly AuthSession _session;

    /// <summary>Initializes a new device authenticator over the given session.</summary>
    /// <param name="session">The session whose token is set on a successful sign-in.</param>
    public DeviceAuthenticator(AuthSession session)
    {
        _session = session;
    }

    /// <summary>Requests a device code, the first step of the device-code flow.</summary>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The device code, the user code and the verification URL.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The service did not return a device code.</exception>
    public async Task<DeviceCode> RequestDeviceCodeAsync(DeviceAuthOptions? options, CancellationToken cancellationToken)
    {
        options ??= new DeviceAuthOptions();

        using var http = CreateHttpClient();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = options.ClientId,
            ["device_id"] = options.DeviceId ?? _session.DeviceId,
            ["device_name"] = options.DeviceName,
        });

        using var response = await http.PostAsync(DeviceCodeUrl, form, cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        DeviceCode? deviceCode = null;
        try
        {
            deviceCode = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<DeviceCode>());
        }
        catch (JsonException)
        {
            // Fall through to the guard below.
        }

        if (deviceCode is null || string.IsNullOrEmpty(deviceCode.Code))
        {
            throw new YandexMusicAuthenticationException(
                "The OAuth service did not return a device code. " + DescribeError(json));
        }

        return deviceCode;
    }

    /// <summary>Polls once for an access token using a previously obtained device code.</summary>
    /// <param name="deviceCode">The <see cref="DeviceCode.Code"/> from <see cref="RequestDeviceCodeAsync"/>.</param>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The access token once the user confirms, or <see langword="null"/> while pending.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The flow failed (for example the code expired or was denied).</exception>
    public static async Task<OAuthToken?> PollDeviceTokenAsync(string deviceCode, DeviceAuthOptions? options, CancellationToken cancellationToken)
    {
        options ??= new DeviceAuthOptions();

        using var http = CreateHttpClient();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "device_code",
            ["code"] = deviceCode,
            ["client_id"] = options.ClientId,
            ["client_secret"] = options.ClientSecret,
        });

        using var response = await http.PostAsync(TokenUrl, form, cancellationToken).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            OAuthToken? token = null;
            try
            {
                token = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<OAuthToken>());
            }
            catch (JsonException)
            {
                // Fall through to the guard below.
            }

            if (token is not null && !string.IsNullOrEmpty(token.AccessToken))
            {
                return token;
            }

            throw new YandexMusicAuthenticationException("The OAuth service returned a success response without an access token.");
        }

        var error = TryReadError(json);
        if (string.Equals(error?.Error, AuthorizationPending, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        throw new YandexMusicAuthenticationException(
            "The OAuth device-code sign-in failed. " + DescribeError(json));
    }

    /// <summary>Runs the full device-code flow until a token is obtained, the code expires, or the operation is cancelled.</summary>
    /// <param name="onCode">A callback invoked with the device code so the user code and URL can be shown to the user.</param>
    /// <param name="options">The flow options, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The obtained access token; the session is also signed in.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The user did not confirm before the code expired.</exception>
    public async Task<OAuthToken> SignInWithDeviceFlowAsync(
        Action<DeviceCode> onCode,
        DeviceAuthOptions? options,
        CancellationToken cancellationToken)
    {
        options ??= new DeviceAuthOptions();

        var deviceCode = await RequestDeviceCodeAsync(options, cancellationToken).ConfigureAwait(false);
        onCode(deviceCode);

        var interval = options.PollInterval ?? TimeSpan.FromSeconds(Math.Max(1, deviceCode.Interval));
        var timeout = options.Timeout ?? TimeSpan.FromSeconds(Math.Max(1, deviceCode.ExpiresIn));

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);

        while (true)
        {
            try
            {
                await Task.Delay(interval, linked.Token).ConfigureAwait(false);
                var token = await PollDeviceTokenAsync(deviceCode.Code, options, linked.Token).ConfigureAwait(false);
                if (token is not null)
                {
                    _session.SetAccessToken(token.AccessToken);
                    return token;
                }
            }
            catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new YandexMusicAuthenticationException(
                    "The device-code sign-in timed out before the user confirmed it.");
            }
        }
    }

    private static OAuthErrorResponse? TryReadError(string json)
    {
        try
        {
            return JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<OAuthErrorResponse>());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DescribeError(string json)
    {
        var error = TryReadError(json);
        if (error?.Error is { Length: > 0 } code)
        {
            return error.ErrorDescription is { Length: > 0 } description
                ? $"OAuth error '{code}': {description}."
                : $"OAuth error '{code}'.";
        }

        return "The response did not contain a recognized OAuth payload.";
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        };

        return new HttpClient(handler, disposeHandler: true);
    }
}
