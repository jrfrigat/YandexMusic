using System.Text.Json.Serialization;

namespace YandexMusic.Authentication;

/// <summary>An OAuth access token issued by the Yandex identity service.</summary>
public sealed class OAuthToken
{
    /// <summary>The access token used to authorize Yandex Music API requests.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>The token used to obtain a fresh access token, if the grant returned one.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>The lifetime of the access token, in seconds, if reported.</summary>
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; init; }

    /// <summary>The token type, usually <c>bearer</c>.</summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }
}

/// <summary>The error body returned by the OAuth endpoints (for example while a sign-in is pending).</summary>
internal sealed class OAuthErrorResponse
{
    /// <summary>The OAuth error code, for example <c>authorization_pending</c>.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>A human-readable description of the error, when present.</summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }
}
