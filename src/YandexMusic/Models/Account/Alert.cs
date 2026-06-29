namespace YandexMusic.Models.Account;

/// <summary>A banner shown to the account (for example a subscription-expiry or gift notice).</summary>
public sealed class Alert
{
    /// <summary>The alert identifier.</summary>
    public string AlertId { get; init; } = string.Empty;

    /// <summary>The alert text.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>The background colour.</summary>
    public string? BgColor { get; init; }

    /// <summary>The text colour.</summary>
    public string? TextColor { get; init; }

    /// <summary>The alert type.</summary>
    public string? AlertType { get; init; }

    /// <summary>The call-to-action button, when present.</summary>
    public AlertButton? Button { get; init; }

    /// <summary>Whether the alert can be dismissed.</summary>
    public bool CloseButton { get; init; }
}

/// <summary>The call-to-action button of an <see cref="Alert"/>.</summary>
public sealed class AlertButton
{
    /// <summary>The button text.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>The background colour.</summary>
    public string? BgColor { get; init; }

    /// <summary>The text colour.</summary>
    public string? TextColor { get; init; }

    /// <summary>The URI the button navigates to.</summary>
    public string? Uri { get; init; }
}
