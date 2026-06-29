using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;

namespace YandexMusic.Models.Presaves;

/// <summary>A user's pre-saved (pre-released) albums, split into upcoming and released sets.</summary>
public sealed class Presaves
{
    /// <summary>Albums that have been pre-saved but are not yet released.</summary>
    [JsonPropertyName("upcoming_albums")]
    public IReadOnlyList<Album>? UpcomingAlbums { get; init; }

    /// <summary>Pre-saved albums that have since been released.</summary>
    [JsonPropertyName("released_albums")]
    public IReadOnlyList<Album>? ReleasedAlbums { get; init; }
}
