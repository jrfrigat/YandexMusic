using System.Text.Json.Serialization;
using YandexMusic.Models.Artists;

namespace YandexMusic.Models;

/// <summary>A call-to-action button shown on an album, artist or playlist page.</summary>
public sealed class ActionButton
{
    /// <summary>The button label.</summary>
    public string? Text { get; init; }

    /// <summary>The link the button opens.</summary>
    public string? Url { get; init; }

    /// <summary>The button color, in HEX.</summary>
    public string? Color { get; init; }

    /// <summary>Whether the link should open in the in-app browser.</summary>
    [JsonPropertyName("viewBrowser")]
    public bool? ViewBrowser { get; init; }
}

/// <summary>"My wave" seeded from an album, artist or playlist, as promoted on its page.</summary>
public sealed class CustomWave
{
    /// <summary>The wave title.</summary>
    public string? Title { get; init; }

    /// <summary>The Lottie animation of the wave.</summary>
    [JsonPropertyName("animationUrl")]
    public string? AnimationUrl { get; init; }

    /// <summary>Where the wave block is placed. Not sent for albums and artists.</summary>
    public string? Position { get; init; }

    /// <summary>The wave headline.</summary>
    public string? Header { get; init; }

    /// <summary>The background image URL.</summary>
    [JsonPropertyName("backgroundImageUrl")]
    public string? BackgroundImageUrl { get; init; }

    /// <summary>The square animation of the wave agent.</summary>
    [JsonPropertyName("squareAgentAnimation")]
    public string? SquareAgentAnimation { get; init; }
}

/// <summary>A link to donate to an artist.</summary>
public sealed class ArtistDonationInfo
{
    /// <summary>The link label.</summary>
    public string? Title { get; init; }

    /// <summary>The donation page URL.</summary>
    public string? Url { get; init; }
}

/// <summary>An announced release of an artist that is not out yet.</summary>
public sealed class UpcomingAlbum
{
    /// <summary>The album identifier.</summary>
    public long? Id { get; init; }

    /// <summary>The album title.</summary>
    public string? Title { get; init; }

    /// <summary>The album type.</summary>
    public string? Type { get; init; }

    /// <summary>The release date.</summary>
    [JsonPropertyName("releaseDate")]
    public DateTimeOffset? ReleaseDate { get; init; }

    /// <summary>The time left until the release, in milliseconds.</summary>
    [JsonPropertyName("millisecondsUntilRelease")]
    public long? MillisecondsUntilRelease { get; init; }

    /// <summary>The cover URI template.</summary>
    [JsonPropertyName("coverUri")]
    public string? CoverUri { get; init; }

    /// <summary>Whether the current user pre-saved the release.</summary>
    public bool? Presaved { get; init; }

    /// <summary>The performing artists.</summary>
    public IReadOnlyList<Artist> Artists { get; init; } = [];
}

/// <summary>Marks a personal playlist and how to address its listener.</summary>
public sealed class MadeForUser
{
    /// <summary>Whether the playlist was made for the current user.</summary>
    [JsonPropertyName("isMadeForUser")]
    public bool? IsMadeForUser { get; init; }

    /// <summary>The listener's name in the grammatical cases the UI needs.</summary>
    [JsonPropertyName("caseForms")]
    public CaseForms? CaseForms { get; init; }
}

/// <summary>A user's name declined in the Russian grammatical cases.</summary>
public sealed class CaseForms
{
    /// <summary>The nominative case.</summary>
    public string? Nominative { get; init; }

    /// <summary>The genitive case.</summary>
    public string? Genitive { get; init; }

    /// <summary>The dative case.</summary>
    public string? Dative { get; init; }

    /// <summary>The accusative case.</summary>
    public string? Accusative { get; init; }

    /// <summary>The instrumental case.</summary>
    public string? Instrumental { get; init; }

    /// <summary>The prepositional case.</summary>
    public string? Prepositional { get; init; }
}

/// <summary>Whether a trailer is available for an album.</summary>
public sealed class AlbumTrailerStatus
{
    /// <summary>Whether a trailer is available.</summary>
    public bool? Available { get; init; }
}
