using System.Text.RegularExpressions;

namespace YandexMusic.Player.Diagnostics;

/// <summary>
/// Strips credentials out of anything on its way into the request journal. A journal is written to
/// be read by someone else — a bug report, a question in an issue — so the account's token, cookies
/// and password must never reach the file in the first place, rather than being cleaned up after.
/// The patterns are deliberately broad: a false positive costs a masked value, a miss costs an
/// account.
/// </summary>
public static partial class Redaction
{
    private const string Mask = "<redacted>";

    /// <summary>Masks every credential-shaped fragment of a line.</summary>
    /// <param name="text">The text about to be logged.</param>
    /// <returns>The text with credentials replaced by a placeholder.</returns>
    public static string Scrub(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        text = OAuthHeader().Replace(text, $"OAuth {Mask}");
        text = BearerHeader().Replace(text, $"Bearer {Mask}");
        text = TokenField().Replace(text, $"$1\": \"{Mask}\"");
        text = TokenQuery().Replace(text, $"$1={Mask}");
        text = CookieHeader().Replace(text, $"$1: {Mask}");
        text = PasswordField().Replace(text, $"$1\": \"{Mask}\"");
        return text;
    }

    [GeneratedRegex(@"OAuth\s+[A-Za-z0-9._~+/\-]+=*", RegexOptions.IgnoreCase)]
    private static partial Regex OAuthHeader();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9._~+/\-]+=*", RegexOptions.IgnoreCase)]
    private static partial Regex BearerHeader();

    [GeneratedRegex("\"((?:access_|refresh_|redirect_)?(?:token|ticket))\"\\s*:\\s*\"[^\"]*\"", RegexOptions.IgnoreCase)]
    private static partial Regex TokenField();

    [GeneratedRegex(@"\b((?:access_|refresh_)?token|passwd|password)=[^&\s""]+", RegexOptions.IgnoreCase)]
    private static partial Regex TokenQuery();

    [GeneratedRegex(@"^(Cookie|Set-Cookie|Authorization)\s*:\s*.+$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex CookieHeader();

    [GeneratedRegex("\"(password|passwd|secret|client_secret)\"\\s*:\\s*\"[^\"]*\"", RegexOptions.IgnoreCase)]
    private static partial Regex PasswordField();
}
