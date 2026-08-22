namespace YandexMusic.Player.Catalog;

/// <summary>A track as the UI needs it — decoupled from the library's model.</summary>
/// <param name="Id">The track identifier.</param>
/// <param name="Title">The track title.</param>
/// <param name="Artist">The display artist(s).</param>
/// <param name="Album">The album title, when known.</param>
/// <param name="Duration">The track duration.</param>
/// <param name="AlbumId">The album identifier, when known; used by play reporting.</param>
public sealed record TrackView(string Id, string Title, string Artist, string? Album, TimeSpan Duration, string? AlbumId = null);

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

/// <summary>An artist summary as the UI needs it.</summary>
/// <param name="Id">The artist identifier, used to fetch the tracks.</param>
/// <param name="Name">The artist name.</param>
/// <param name="TrackCount">How many tracks the catalogue holds for the artist, or 0 when unknown.</param>
public sealed record ArtistView(string Id, string Name, int TrackCount);

/// <summary>An artist together with the tracks the player can start from.</summary>
/// <param name="Artist">The artist summary.</param>
/// <param name="Tracks">The artist's most popular tracks.</param>
public sealed record ArtistDetail(ArtistView Artist, IReadOnlyList<TrackView> Tracks);

/// <summary>A playlist summary as the UI needs it.</summary>
/// <param name="Id">The playlist kind (unique per owner), used to fetch it.</param>
/// <param name="Title">The playlist title.</param>
/// <param name="TrackCount">The number of tracks.</param>
public sealed record PlaylistView(string Id, string Title, int TrackCount);

/// <summary>A playlist together with its tracklist.</summary>
/// <param name="Playlist">The playlist summary.</param>
/// <param name="Tracks">The playlist's tracks, in order.</param>
public sealed record PlaylistDetail(PlaylistView Playlist, IReadOnlyList<TrackView> Tracks);

/// <summary>A batch of tracks from a radio station, with the identity needed to report feedback.</summary>
/// <param name="Tracks">The tracks of the batch.</param>
/// <param name="Station">The station identity, for example "user:onyourwave" or "track:{id}".</param>
/// <param name="BatchId">The batch identifier, echoed back in radio feedback.</param>
public sealed record RadioBatch(IReadOnlyList<TrackView> Tracks, string Station, string BatchId);

/// <summary>One page of search results together with the total number of matches.</summary>
/// <param name="Items">The items on this page.</param>
/// <param name="Total">The total number of matches across all pages.</param>
public sealed record SearchPage<T>(IReadOnlyList<T> Items, int Total)
    where T : class;
