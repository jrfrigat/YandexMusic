using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Playlists;

/// <summary>The personalized track recommendations attached to a playlist.</summary>
public sealed class PlaylistRecommendations
{
    /// <summary>The recommended tracks.</summary>
    public IReadOnlyList<Track> Tracks { get; init; } = [];

    /// <summary>The batch identifier used to report feedback about these recommendations, when present.</summary>
    public string? BatchId { get; init; }
}
