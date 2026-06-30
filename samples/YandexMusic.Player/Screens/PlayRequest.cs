using YandexMusic.Player.Catalog;

namespace YandexMusic.Player.Screens;

/// <summary>A request to start playback of a list of tracks from a given index (returned by screens).</summary>
/// <param name="Tracks">The tracks to enqueue (for example a set of search results or an album).</param>
/// <param name="StartIndex">The index within <paramref name="Tracks"/> to start playing from.</param>
public sealed record PlayRequest(IReadOnlyList<TrackView> Tracks, int StartIndex);
