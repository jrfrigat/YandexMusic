using System.Text.Json.Serialization;

namespace YandexMusic.Authentication;

/// <summary>
/// The result of starting an OAuth device-code sign-in. Show <see cref="UserCode"/> and
/// <see cref="VerificationUrl"/> to the user, then poll for a token with <see cref="Code"/>.
/// </summary>
public sealed class DeviceCode
{
    /// <summary>The opaque device code used to poll for an access token.</summary>
    [JsonPropertyName("device_code")]
    public string Code { get; init; } = string.Empty;

    /// <summary>The short code the user enters on the confirmation page.</summary>
    [JsonPropertyName("user_code")]
    public string UserCode { get; init; } = string.Empty;

    /// <summary>The URL of the page where the user confirms the sign-in.</summary>
    [JsonPropertyName("verification_url")]
    public string VerificationUrl { get; init; } = string.Empty;

    /// <summary>The number of seconds until <see cref="Code"/> expires.</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>The server-recommended interval, in seconds, between polling attempts.</summary>
    [JsonPropertyName("interval")]
    public int Interval { get; init; }
}
