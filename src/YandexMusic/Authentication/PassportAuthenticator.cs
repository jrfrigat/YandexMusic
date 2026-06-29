using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace YandexMusic.Authentication;

/// <summary>
/// Implements the interactive Yandex Passport sign-in flows (cookie exchange and QR code) and the
/// exchange of a Yandex session into a Yandex Music OAuth token.
/// </summary>
/// <remarks>
/// ⚠️ Best-effort implementation of a version-sensitive, undocumented protocol. The endpoints and
/// payload shapes can change without notice and could not be validated from the build environment —
/// verify against the live flow and adjust if Yandex has changed the protocol. The token sign-in
/// (<see cref="IAuthenticationClient.SignInWithToken"/>) is the robust, fully-supported path.
/// </remarks>
internal sealed partial class PassportAuthenticator
{
    // The public Yandex Music application identifier (the same value embedded in every Yandex Music client).
    private const string MusicClientId = "23cabbbdc6cd418abb4b39c32c41195d";

    private const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Safari/537.36";

    private readonly AuthSession _session;

    /// <summary>Initializes a new authenticator over the given session.</summary>
    /// <param name="session">The session whose cookie container and token are managed.</param>
    public PassportAuthenticator(AuthSession session)
    {
        _session = session;
    }

    /// <summary>Adds the supplied cookies to the session and exchanges them for an access token.</summary>
    /// <param name="cookies">Yandex session cookies (for the <c>.yandex.ru</c> domain).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    public async Task SignInWithCookiesAsync(IEnumerable<Cookie> cookies, CancellationToken cancellationToken)
    {
        foreach (var cookie in cookies)
        {
            _session.Cookies.Add(cookie);
        }

        await ExchangeCookiesForTokenAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Exchanges the current session cookies for a Yandex Music access token via the OAuth implicit flow.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <exception cref="YandexMusicAuthenticationException">No token could be obtained from the cookies.</exception>
    public async Task ExchangeCookiesForTokenAsync(CancellationToken cancellationToken)
    {
        using var http = CreateHttpClient();
        using var response = await http
            .GetAsync($"https://oauth.yandex.ru/authorize?response_type=token&client_id={MusicClientId}", cancellationToken)
            .ConfigureAwait(false);

        var token = ExtractAccessToken(response.Headers.Location?.ToString());
        if (string.IsNullOrEmpty(token))
        {
            throw new YandexMusicAuthenticationException(
                "Could not obtain an access token from the Yandex session cookies. Ensure the cookies are valid and the Music app is authorized for the account.");
        }

        _session.SetAccessToken(token);
    }

    /// <summary>Starts a QR sign-in by creating a Passport session and a magic-link URL.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The in-progress sign-in to render and poll.</returns>
    /// <exception cref="YandexMusicAuthenticationException">The Passport session could not be created.</exception>
    public async Task<QrSignIn> StartQrSignInAsync(CancellationToken cancellationToken)
    {
        using var http = CreateHttpClient();

        var page = await http.GetStringAsync("https://passport.yandex.ru/auth", cancellationToken).ConfigureAwait(false);
        var csrf = CsrfTokenRegex().Match(page) is { Success: true } m1 ? m1.Groups[1].Value : null;
        var processUuid = ProcessUuidRegex().Match(page) is { Success: true } m2 ? m2.Groups[1].Value : null;

        if (string.IsNullOrEmpty(csrf) || string.IsNullOrEmpty(processUuid))
        {
            throw new YandexMusicAuthenticationException("Could not initialize a Yandex Passport sign-in session.");
        }

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["csrf_token"] = csrf,
            ["retpath"] = "https://passport.yandex.ru/profile",
            ["with_code"] = "1",
        });

        using var response = await http
            .PostAsync("https://passport.yandex.ru/registration-validations/auth/password/submit", form, cancellationToken)
            .ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var trackId = root.TryGetProperty("track_id", out var t) ? t.GetString() : null;
        var sessionCsrf = root.TryGetProperty("csrf_token", out var c) ? c.GetString() : csrf;

        if (string.IsNullOrEmpty(trackId))
        {
            throw new YandexMusicAuthenticationException("Yandex Passport did not return a sign-in track id.");
        }

        return new QrSignIn
        {
            Url = $"https://passport.yandex.ru/auth/magic/code/?track_id={trackId}",
            TrackId = trackId,
            CsrfToken = sessionCsrf ?? csrf,
            ProcessUuid = processUuid,
        };
    }

    /// <summary>Polls a QR sign-in once; on success the session is authenticated.</summary>
    /// <param name="qrSignIn">The in-progress sign-in returned by <see cref="StartQrSignInAsync"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the user has confirmed and a token was obtained.</returns>
    public async Task<bool> TryCompleteQrSignInAsync(QrSignIn qrSignIn, CancellationToken cancellationToken)
    {
        using var http = CreateHttpClient();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["csrf_token"] = qrSignIn.CsrfToken,
            ["track_id"] = qrSignIn.TrackId,
            ["process_uuid"] = qrSignIn.ProcessUuid,
        });

        using var response = await http
            .PostAsync("https://passport.yandex.ru/auth/new/magic/status/", form, cancellationToken)
            .ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!IsQrConfirmed(json))
        {
            return false;
        }

        await ExchangeCookiesForTokenAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>Extracts the <c>access_token</c> from an OAuth implicit-flow redirect URL fragment.</summary>
    /// <param name="redirectLocation">The <c>Location</c> header of the OAuth redirect.</param>
    /// <returns>The access token, or <see langword="null"/> if the URL does not contain one.</returns>
    internal static string? ExtractAccessToken(string? redirectLocation)
    {
        if (string.IsNullOrEmpty(redirectLocation))
        {
            return null;
        }

        var hash = redirectLocation.IndexOf('#');
        var fragment = hash >= 0 ? redirectLocation[(hash + 1)..] : redirectLocation;

        foreach (var pair in fragment.Split('&'))
        {
            var separator = pair.IndexOf('=');
            if (separator > 0 && pair[..separator] == "access_token")
            {
                return Uri.UnescapeDataString(pair[(separator + 1)..]);
            }
        }

        return null;
    }

    /// <summary>Determines whether a QR status response indicates the user has confirmed the sign-in.</summary>
    /// <param name="statusJson">The Passport magic-status JSON.</param>
    /// <returns><see langword="true"/> when the sign-in is confirmed.</returns>
    internal static bool IsQrConfirmed(string statusJson)
    {
        try
        {
            using var document = JsonDocument.Parse(statusJson);
            var root = document.RootElement;

            // The sign-in is still pending while errors are present (for example "track.not_found").
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
            {
                return false;
            }

            var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
            return string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            CookieContainer = _session.Cookies,
            UseCookies = true,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
        };

        var http = new HttpClient(handler, disposeHandler: true);
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", BrowserUserAgent);
        return http;
    }

    [GeneratedRegex("\"csrf_token\"\\s*:\\s*\"([^\"]+)\"")]
    private static partial Regex CsrfTokenRegex();

    [GeneratedRegex("\"process_uuid\"\\s*:\\s*\"([^\"]+)\"")]
    private static partial Regex ProcessUuidRegex();
}
