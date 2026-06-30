using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Account;

/// <summary>The subscription purchase offer and payment configuration for the signed-in account.</summary>
public sealed class Settings
{
    /// <summary>The products purchasable through in-app billing.</summary>
    public IReadOnlyList<Product> InAppProducts { get; init; } = [];

    /// <summary>The products purchasable outside in-app billing (for example via the web).</summary>
    public IReadOnlyList<Product> NativeProducts { get; init; } = [];

    /// <summary>The URL used to start a web payment.</summary>
    public string? WebPaymentUrl { get; init; }

    /// <summary>Whether promo codes can be redeemed for this account.</summary>
    public bool PromoCodesEnabled { get; init; }

    /// <summary>The price of the monthly product offered through web payment, when present.</summary>
    public Price? WebPaymentMonthProductPrice { get; init; }
}

/// <summary>A purchasable subscription product.</summary>
public sealed class Product
{
    /// <summary>The product identifier.</summary>
    public string ProductId { get; init; } = string.Empty;

    /// <summary>The product type.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The duration of the subscription period.</summary>
    public int Duration { get; init; }

    /// <summary>The duration of the trial period.</summary>
    public int TrialDuration { get; init; }

    /// <summary>The feature unlocked by the product.</summary>
    public string Feature { get; init; } = string.Empty;

    /// <summary>Whether this is a debug product.</summary>
    public bool Debug { get; init; }

    /// <summary>Whether the product grants Yandex Plus.</summary>
    public bool Plus { get; init; }

    /// <summary>The price, when present.</summary>
    public Price? Price { get; init; }

    /// <summary>The ISO-8601 duration of the standard billing period, when present.</summary>
    public string? CommonPeriodDuration { get; init; }

    /// <summary>Whether this is the cheapest available product, when reported.</summary>
    public bool? Cheapest { get; init; }

    /// <summary>The display title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>Whether this is a family subscription product, when reported.</summary>
    public bool? FamilySub { get; init; }

    /// <summary>The Facebook share image URL, when present.</summary>
    public string? FbImage { get; init; }

    /// <summary>The Facebook share name, when present.</summary>
    public string? FbName { get; init; }

    /// <summary>Whether this is a family product, when reported.</summary>
    public bool? Family { get; init; }

    /// <summary>The included features, when present.</summary>
    public IReadOnlyList<string>? Features { get; init; }

    /// <summary>The description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>Whether the product is available for purchase, when reported.</summary>
    public bool? Available { get; init; }

    /// <summary>Whether a trial is available, when reported.</summary>
    public bool? TrialAvailable { get; init; }

    /// <summary>The ISO-8601 duration of the trial period, when present.</summary>
    public string? TrialPeriodDuration { get; init; }

    /// <summary>The ISO-8601 duration of the introductory period, when present.</summary>
    public string? IntroPeriodDuration { get; init; }

    /// <summary>The introductory price, when present.</summary>
    public Price? IntroPrice { get; init; }

    /// <summary>The ISO-8601 duration of the start period, when present.</summary>
    public string? StartPeriodDuration { get; init; }

    /// <summary>The start-period price, when present.</summary>
    public Price? StartPrice { get; init; }

    /// <summary>The license text fragments, when present.</summary>
    public IReadOnlyList<LicenceTextPart>? LicenceTextParts { get; init; }

    /// <summary>Whether a vendor trial is available, when reported.</summary>
    public bool? VendorTrialAvailable { get; init; }

    /// <summary>The call-to-action button text, when present.</summary>
    public string? ButtonText { get; init; }

    /// <summary>The secondary call-to-action button text, when present.</summary>
    public string? ButtonAdditionalText { get; init; }

    /// <summary>The supported payment method types, when present.</summary>
    public IReadOnlyList<string>? PaymentMethodTypes { get; init; }
}

/// <summary>A monetary amount in a given currency.</summary>
public sealed class Price
{
    /// <summary>The amount in the currency's units. The API may report this as an integer, a decimal or a string.</summary>
    [JsonConverter(typeof(FlexibleDecimalConverter))]
    public decimal Amount { get; init; }

    /// <summary>The ISO-4217 currency code.</summary>
    public string Currency { get; init; } = string.Empty;
}

/// <summary>A fragment of license text, optionally linking to a document.</summary>
public sealed class LicenceTextPart
{
    /// <summary>The text fragment.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>The URL the fragment links to, when present.</summary>
    public string? Url { get; init; }
}
