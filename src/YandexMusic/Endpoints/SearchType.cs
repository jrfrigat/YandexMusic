namespace YandexMusic.Endpoints;

/// <summary>Restricts a catalogue search to a single content category, or searches all of them.</summary>
public enum SearchType
{
    /// <summary>Search every category.</summary>
    All,

    /// <summary>Search tracks only.</summary>
    Track,

    /// <summary>Search albums only.</summary>
    Album,

    /// <summary>Search artists only.</summary>
    Artist,

    /// <summary>Search playlists only.</summary>
    Playlist,

    /// <summary>Search podcasts only.</summary>
    Podcast,

    /// <summary>Search podcast episodes only.</summary>
    PodcastEpisode,

    /// <summary>Search videos only.</summary>
    Video,

    /// <summary>Search users only.</summary>
    User,
}
