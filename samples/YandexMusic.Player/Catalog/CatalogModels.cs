namespace YandexMusic.Player.Catalog;

/// <summary>A track as the UI needs it — decoupled from the library's model.</summary>
/// <param name="Id">The track identifier.</param>
/// <param name="Title">The track title.</param>
/// <param name="Artist">The display artist(s).</param>
/// <param name="Album">The album title, when known.</param>
/// <param name="Duration">The track duration.</param>
public sealed record TrackView(string Id, string Title, string Artist, string? Album, TimeSpan Duration);

/// <summary>An album summary as the UI needs it.</summary>
/// <param name="Id">The album identifier.</param>
/// <param name="Title">The album title.</param>
/// <param name="Artist">The display artist(s).</param>
/// <param name="Year">The release year, when known.</param>
/// <param name="TrackCount">The number of tracks.</param>
public sealed record AlbumView(string Id, string Title, string Artist, int? Year, int TrackCount);

/// <summary>An album together with its tracklist.</summary>
/// <param name="Album">The album summary.</param>
/// <param name="Tracks">The album's tracks, in order.</param>
public sealed record AlbumDetail(AlbumView Album, IReadOnlyList<TrackView> Tracks);
