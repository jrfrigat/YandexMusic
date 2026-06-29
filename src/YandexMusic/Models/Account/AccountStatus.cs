namespace YandexMusic.Models.Account;

/// <summary>The signed-in account together with its subscription, Plus status and permissions.</summary>
public sealed class AccountStatus
{
    /// <summary>The account information.</summary>
    public AccountInfo Account { get; init; } = new();

    /// <summary>The subscription permissions, when present.</summary>
    public Permissions? Permissions { get; init; }

    /// <summary>The subscription state, when present.</summary>
    public Subscription? Subscription { get; init; }

    /// <summary>The Yandex Plus status, when present.</summary>
    public Plus? Plus { get; init; }

    /// <summary>The advertisement policy for the account, when present.</summary>
    public string? Advertisement { get; init; }

    /// <summary>The default account e-mail, when present.</summary>
    public string? DefaultEmail { get; init; }

    /// <summary>The number of allowed radio skips per hour, when reported.</summary>
    public int? SkipsPerHour { get; init; }

    /// <summary>Whether the account has an existing radio station, when reported.</summary>
    public bool? StationExists { get; init; }

    /// <summary>The premium region identifier, when reported.</summary>
    public int? PremiumRegion { get; init; }

    /// <summary>Whether a pre-trial offer is active, when reported.</summary>
    public bool? PretrialActive { get; init; }

    /// <summary>The cached-track limit for the account, when reported.</summary>
    public int? CacheLimit { get; init; }

    /// <summary>The banner shown below the player (for example a subscription or gift notice), when present.</summary>
    public Alert? BarBelow { get; init; }
}

/// <summary>Information about a Yandex account.</summary>
public sealed class AccountInfo
{
    /// <summary>The account identifier. <c>0</c> for an anonymous session.</summary>
    public long Uid { get; init; }

    /// <summary>The login, when authenticated.</summary>
    public string? Login { get; init; }

    /// <summary>The full name, when authenticated.</summary>
    public string? FullName { get; init; }

    /// <summary>The first name, when authenticated.</summary>
    public string? FirstName { get; init; }

    /// <summary>The second name, when authenticated.</summary>
    public string? SecondName { get; init; }

    /// <summary>The display name, when authenticated.</summary>
    public string? DisplayName { get; init; }

    /// <summary>The numeric region identifier.</summary>
    public int Region { get; init; }

    /// <summary>The current server time, as the API reports it (a free-form timestamp string).</summary>
    public string? Now { get; init; }

    /// <summary>Whether the Yandex Music service is available in the account's region.</summary>
    public bool ServiceAvailable { get; init; }

    /// <summary>Whether this is a hosted (organisation) user.</summary>
    public bool HostedUser { get; init; }

    /// <summary>The birthday, when set.</summary>
    public string? Birthday { get; init; }

    /// <summary>When the account was registered, when reported.</summary>
    public string? RegisteredAt { get; init; }

    /// <summary>Whether this is a child account, when reported.</summary>
    public bool? Child { get; init; }

    /// <summary>The phone numbers attached to the passport account.</summary>
    public IReadOnlyList<PassportPhone>? PassportPhones { get; init; }
}

/// <summary>The feature permissions granted by an account's subscription.</summary>
public sealed class Permissions
{
    /// <summary>When the current permissions expire.</summary>
    public DateTimeOffset? Until { get; init; }

    /// <summary>The currently granted permission values.</summary>
    public IReadOnlyList<string> Values { get; init; } = [];

    /// <summary>The default permission values.</summary>
    public IReadOnlyList<string> Default { get; init; } = [];
}
