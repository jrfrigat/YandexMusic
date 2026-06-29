namespace YandexMusic.Models;

/// <summary>A Yandex user, as referenced by playlist owners and similar entities.</summary>
public sealed class User
{
    /// <summary>The user identifier.</summary>
    public long Uid { get; init; }

    /// <summary>The user login.</summary>
    public string? Login { get; init; }

    /// <summary>The display name.</summary>
    public string? Name { get; init; }

    /// <summary>The reported sex (<c>male</c>, <c>female</c> or <c>unknown</c>).</summary>
    public string? Sex { get; init; }

    /// <summary>Whether the user is verified.</summary>
    public bool Verified { get; init; }
}
