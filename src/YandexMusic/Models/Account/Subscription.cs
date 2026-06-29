namespace YandexMusic.Models.Account;

/// <summary>The Yandex Plus status of an account.</summary>
public sealed class Plus
{
    /// <summary>Whether the account currently has Yandex Plus.</summary>
    public bool HasPlus { get; init; }

    /// <summary>Whether the Plus onboarding tutorial has been completed.</summary>
    public bool IsTutorialCompleted { get; init; }
}

/// <summary>The subscription state of an account.</summary>
public sealed class Subscription
{
    /// <summary>Whether the account has ever had any subscription.</summary>
    public bool HadAnySubscription { get; init; }

    /// <summary>The active auto-renewing subscriptions.</summary>
    public IReadOnlyList<AutoRenewable>? AutoRenewable { get; init; }

    /// <summary>The active family auto-renewing subscriptions.</summary>
    public IReadOnlyList<AutoRenewable>? FamilyAutoRenewable { get; init; }

    /// <summary>The active non-renewing subscription, when present.</summary>
    public NonAutoRenewable? NonAutoRenewable { get; init; }

    /// <summary>How long a non-renewing subscription has left, when present.</summary>
    public RenewableRemainder? NonAutoRenewableRemainder { get; init; }

    /// <summary>The mobile-carrier billing operators for the subscription, when billed via a carrier.</summary>
    public IReadOnlyList<Operator>? Operator { get; init; }

    /// <summary>Whether the account can start a trial.</summary>
    public bool? CanStartTrial { get; init; }

    /// <summary>When the subscription ends, when applicable.</summary>
    public string? End { get; init; }
}

/// <summary>A mobile-carrier billing operator for a subscription.</summary>
public sealed class Operator
{
    /// <summary>The product identifier billed through the operator.</summary>
    public string ProductId { get; init; } = string.Empty;

    /// <summary>The phone number the subscription is billed to.</summary>
    public string Phone { get; init; } = string.Empty;

    /// <summary>The billing regularity (for example <c>monthly</c>).</summary>
    public string PaymentRegularity { get; init; } = string.Empty;

    /// <summary>The ways the subscription can be deactivated with this operator.</summary>
    public IReadOnlyList<Deactivation>? Deactivation { get; init; }

    /// <summary>The operator's display title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Whether billing is currently suspended.</summary>
    public bool Suspended { get; init; }
}

/// <summary>A way to deactivate a carrier-billed subscription.</summary>
public sealed class Deactivation
{
    /// <summary>The deactivation method.</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Human-readable deactivation instructions, when provided.</summary>
    public string? Instructions { get; init; }
}

/// <summary>An auto-renewing subscription.</summary>
public sealed class AutoRenewable
{
    /// <summary>When the current period expires.</summary>
    public string Expires { get; init; } = string.Empty;

    /// <summary>The payment vendor (for example <c>AppStore</c> or <c>Google</c>).</summary>
    public string Vendor { get; init; } = string.Empty;

    /// <summary>A help URL for managing the subscription with the vendor.</summary>
    public string VendorHelpUrl { get; init; } = string.Empty;

    /// <summary>The subscribed product, when present.</summary>
    public Product? Product { get; init; }

    /// <summary>Whether the subscription has finished (will not renew).</summary>
    public bool Finished { get; init; }

    /// <summary>The owner of a family subscription, when this membership is shared.</summary>
    public User? MasterInfo { get; init; }

    /// <summary>The product identifier, when present.</summary>
    public string? ProductId { get; init; }

    /// <summary>The order identifier, when present.</summary>
    public long? OrderId { get; init; }
}

/// <summary>A non-renewing (one-off) subscription period.</summary>
public sealed class NonAutoRenewable
{
    /// <summary>When the period starts.</summary>
    public string Start { get; init; } = string.Empty;

    /// <summary>When the period ends.</summary>
    public string End { get; init; } = string.Empty;
}

/// <summary>The remaining time of a non-renewing subscription.</summary>
public sealed class RenewableRemainder
{
    /// <summary>The number of days remaining.</summary>
    public int Days { get; init; }
}

/// <summary>A phone number attached to a Yandex passport account.</summary>
public sealed class PassportPhone
{
    /// <summary>The phone number.</summary>
    public string Phone { get; init; } = string.Empty;
}
