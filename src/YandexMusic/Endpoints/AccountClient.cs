using YandexMusic.Http;
using YandexMusic.Models.Account;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IAccountClient"/> implementation.</summary>
internal sealed class AccountClient : IAccountClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new account endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public AccountClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<AccountStatus?> GetStatusAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<AccountStatus>("/account/status", cancellationToken);

    /// <inheritdoc />
    public Task<UserSettings?> GetSettingsAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<UserSettings>("/account/settings", cancellationToken);

    /// <inheritdoc />
    public Task<UserSettings?> SetSettingsAsync(string name, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        return SetSettingsAsync(new Dictionary<string, string> { [name] = value }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<UserSettings?> SetSettingsAsync(IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var content = new FormUrlEncodedContent(data);
        return _connection.PostAsync<UserSettings>("/account/settings", content, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Settings?> GetPurchaseSettingsAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<Settings>("/settings", cancellationToken);

    /// <inheritdoc />
    public Task<PermissionAlerts?> GetPermissionAlertsAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<PermissionAlerts>("/permission-alerts", cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>> GetExperimentsAsync(CancellationToken cancellationToken = default)
    {
        var experiments = await _connection
            .GetAsync<Dictionary<string, string>>("/account/experiments", cancellationToken)
            .ConfigureAwait(false);

        return experiments ?? (IReadOnlyDictionary<string, string>)new Dictionary<string, string>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ExperimentDetail>> GetExperimentsDetailsAsync(CancellationToken cancellationToken = default)
    {
        var details = await _connection
            .GetAsync<Dictionary<string, ExperimentDetail>>("/account/experiments/details", cancellationToken)
            .ConfigureAwait(false);

        return details ?? (IReadOnlyDictionary<string, ExperimentDetail>)new Dictionary<string, ExperimentDetail>();
    }

    /// <inheritdoc />
    public async Task<PromoCodeStatus?> ConsumePromoCodeAsync(string code, string? language = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        // The reference always sends a language; default to the API's default locale (Russian).
        var fields = new Dictionary<string, string>
        {
            ["code"] = code,
            ["language"] = string.IsNullOrEmpty(language) ? "ru" : language,
        };

        using var content = new FormUrlEncodedContent(fields);
        return await _connection
            .PostAsync<PromoCodeStatus>("/account/consume-promo-code", content, cancellationToken)
            .ConfigureAwait(false);
    }
}
