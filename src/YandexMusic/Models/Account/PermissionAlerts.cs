namespace YandexMusic.Models.Account;

/// <summary>The subscription permission alerts shown to the signed-in account.</summary>
public sealed class PermissionAlerts
{
    /// <summary>The alert messages.</summary>
    public IReadOnlyList<string> Alerts { get; init; } = [];
}
