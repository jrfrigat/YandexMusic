using YandexMusic.Models.Account;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the signed-in account.</summary>
public interface IAccountClient
{
    /// <summary>Gets the status of the signed-in account, including subscription permissions.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The account status.</returns>
    Task<AccountStatus?> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the signed-in user's playback and privacy settings.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user settings.</returns>
    Task<UserSettings?> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Updates a single user setting.</summary>
    /// <param name="name">The camelCase setting name (a field of <see cref="UserSettings"/>).</param>
    /// <param name="value">The setting value, serialized to a string.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated user settings.</returns>
    Task<UserSettings?> SetSettingsAsync(string name, string value, CancellationToken cancellationToken = default);

    /// <summary>Updates several user settings at once.</summary>
    /// <param name="data">The camelCase setting name to string value pairs to apply.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated user settings.</returns>
    Task<UserSettings?> SetSettingsAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default);

    /// <summary>Gets the subscription purchase offers and payment configuration.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The purchase settings.</returns>
    Task<Settings?> GetPurchaseSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the subscription permission alerts for the signed-in account.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The permission alerts.</returns>
    Task<PermissionAlerts?> GetPermissionAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the A/B experiments the account participates in, as a name to value map.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The experiment name to value map.</returns>
    Task<IReadOnlyDictionary<string, string>> GetExperimentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets detailed configuration for the account's A/B experiments, keyed by experiment name.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The experiment name to detail map.</returns>
    Task<IReadOnlyDictionary<string, ExperimentDetail>> GetExperimentsDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>Redeems a promo code for the signed-in account.</summary>
    /// <param name="code">The promo code to redeem.</param>
    /// <param name="language">The ISO 639-1 response language; defaults to Russian (<c>ru</c>) when not specified.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The redemption result.</returns>
    Task<PromoCodeStatus?> ConsumePromoCodeAsync(string code, string? language = null, CancellationToken cancellationToken = default);
}
