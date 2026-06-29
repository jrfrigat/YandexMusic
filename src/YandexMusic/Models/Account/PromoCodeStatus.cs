using System.Text.Json.Serialization;

namespace YandexMusic.Models.Account;

/// <summary>The outcome of redeeming a promo code.</summary>
public sealed class PromoCodeStatus
{
    /// <summary>The machine-readable status of the redemption.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>The human-readable status description.</summary>
    public string StatusDesc { get; init; } = string.Empty;

    /// <summary>The refreshed account status after redemption, when present.</summary>
    [JsonPropertyName("account_status")]
    public AccountStatus? AccountStatus { get; init; }
}
