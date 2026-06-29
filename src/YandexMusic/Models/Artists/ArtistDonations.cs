namespace YandexMusic.Models.Artists;

/// <summary>The result of the artist "artist-donation" block: the donation entries for an artist.</summary>
public sealed class ArtistDonations
{
    /// <summary>The donation entries.</summary>
    public IReadOnlyList<ArtistDonationItem>? Donations { get; init; }
}

/// <summary>An entry within an <see cref="ArtistDonations"/> result.</summary>
public sealed class ArtistDonationItem
{
    /// <summary>The item type (for example <c>donation_item</c>).</summary>
    public string? Type { get; init; }

    /// <summary>The item payload, when present.</summary>
    public ArtistDonationData? Data { get; init; }
}

/// <summary>The payload of an <see cref="ArtistDonationItem"/>.</summary>
public sealed class ArtistDonationData
{
    /// <summary>The URL of the page where the user can tip the artist, when present.</summary>
    public string? TipUrl { get; init; }

    /// <summary>The artist being supported, when present.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The donation goal, when present.</summary>
    public ArtistDonationGoal? Goal { get; init; }
}

/// <summary>A donation goal advertised for an artist.</summary>
public sealed class ArtistDonationGoal
{
    /// <summary>The goal title, when present.</summary>
    public string? Title { get; init; }
}
