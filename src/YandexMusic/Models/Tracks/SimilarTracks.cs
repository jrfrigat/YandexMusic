using System.Text.Json.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>A track together with the tracks the catalogue considers similar to it.</summary>
public sealed class SimilarTracks
{
    /// <summary>The track the similarity is computed for.</summary>
    public Track? Track { get; init; }

    /// <summary>The similar tracks.</summary>
    [JsonPropertyName("similarTracks")]
    public IReadOnlyList<Track> Tracks { get; init; } = [];
}
