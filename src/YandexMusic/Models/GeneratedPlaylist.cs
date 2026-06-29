using System.Text.Json;
using YandexMusic.Models.Playlists;

namespace YandexMusic.Models;

/// <summary>
/// An auto-generated playlist surfaced on the landing page or in playlist recommendations (for
/// example "Playlist of the day").
/// </summary>
public sealed class GeneratedPlaylist
{
    /// <summary>
    /// The generator type. Known values include <c>playlistOfTheDay</c>, <c>origin</c>,
    /// <c>recentTracks</c>, <c>neverHeard</c>, <c>podcasts</c>, <c>missedLikes</c>.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Whether the playlist has finished generating.</summary>
    public bool Ready { get; init; }

    /// <summary>Whether the user should be notified about the playlist.</summary>
    public bool Notify { get; init; }

    /// <summary>The generated playlist, when present.</summary>
    public Playlist? Data { get; init; }

    /// <summary>The structured description blocks, when present.</summary>
    public IReadOnlyList<JsonElement>? Description { get; init; }

    /// <summary>A short preview description, when present.</summary>
    public string? PreviewDescription { get; init; }
}
