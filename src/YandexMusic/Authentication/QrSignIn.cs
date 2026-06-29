namespace YandexMusic.Authentication;

/// <summary>
/// An in-progress QR sign-in. Render <see cref="Url"/> as a QR code for the user to scan with the
/// Yandex app, then poll <see cref="IAuthenticationClient.TryCompleteQrSignInAsync"/> until it succeeds.
/// </summary>
public sealed class QrSignIn
{
    /// <summary>The URL to render as a QR code (and/or open) for the user to confirm the sign-in.</summary>
    public required string Url { get; init; }

    /// <summary>The Passport track identifier for this sign-in attempt.</summary>
    internal string TrackId { get; init; } = string.Empty;

    /// <summary>The CSRF token bound to this sign-in attempt.</summary>
    internal string CsrfToken { get; init; } = string.Empty;

    /// <summary>The Passport process identifier for this sign-in attempt.</summary>
    internal string ProcessUuid { get; init; } = string.Empty;
}
