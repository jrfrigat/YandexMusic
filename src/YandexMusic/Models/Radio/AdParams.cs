using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>The advertising parameters supplied alongside a station.</summary>
public sealed class AdParams
{
    /// <summary>The advertising partner identifier.</summary>
    [JsonPropertyName("partnerId")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string PartnerId { get; init; } = string.Empty;

    /// <summary>The advertising category identifier.</summary>
    [JsonPropertyName("categoryId")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string CategoryId { get; init; } = string.Empty;

    /// <summary>The page reference for the ad slot.</summary>
    [JsonPropertyName("pageRef")]
    public string PageRef { get; init; } = string.Empty;

    /// <summary>The target reference for the ad slot.</summary>
    [JsonPropertyName("targetRef")]
    public string TargetRef { get; init; } = string.Empty;

    /// <summary>Additional opaque parameters passed to the ad provider.</summary>
    [JsonPropertyName("otherParams")]
    public string OtherParams { get; init; } = string.Empty;

    /// <summary>The volume to apply to the advertisement.</summary>
    [JsonPropertyName("adVolume")]
    public int AdVolume { get; init; }

    /// <summary>The genre identifier targeted by the ad, when present.</summary>
    [JsonPropertyName("genreId")]
    public string? GenreId { get; init; }

    /// <summary>The genre name targeted by the ad, when present.</summary>
    [JsonPropertyName("genreName")]
    public string? GenreName { get; init; }
}
