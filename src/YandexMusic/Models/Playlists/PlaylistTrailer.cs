namespace YandexMusic.Models.Playlists;

/// <summary>The trailer (a short preview) configured for a playlist.</summary>
public sealed class PlaylistTrailer
{
    /// <summary>The playlist the trailer belongs to, when present.</summary>
    public Playlist? Playlist { get; init; }

    /// <summary>The trailer payload, when present.</summary>
    public TrailerInfo? Trailer { get; init; }

    /// <summary>Whether the trailer can be shared, when reported.</summary>
    public bool? Shareable { get; init; }
}
