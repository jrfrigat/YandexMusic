namespace YandexMusic.Models.Playlists;

/// <summary>A container of playlists returned when fetching several playlists by their identifiers.</summary>
public sealed class PlaylistsList
{
    /// <summary>The playlists, when present.</summary>
    public IReadOnlyList<Playlist>? Playlists { get; init; }
}
