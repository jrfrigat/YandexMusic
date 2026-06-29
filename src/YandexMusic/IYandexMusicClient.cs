using YandexMusic.Authentication;
using YandexMusic.Endpoints;

namespace YandexMusic;

/// <summary>
/// The entry point to the Yandex Music API. Exposes authentication and, as they are implemented,
/// the typed endpoint groups (for example <c>Tracks</c>, <c>Search</c> and <c>Playlists</c>).
/// Create one directly with <see cref="YandexMusicClient"/> or resolve it from dependency injection.
/// </summary>
public interface IYandexMusicClient : IDisposable, IAsyncDisposable
{
    /// <summary>Manages authentication for this client (sign-in, sign-out and session state).</summary>
    IAuthenticationClient Authentication { get; }

    /// <summary>Endpoints for retrieving tracks from the catalogue.</summary>
    ITracksClient Tracks { get; }

    /// <summary>Endpoints for searching the catalogue.</summary>
    ISearchClient Search { get; }

    /// <summary>Endpoints for retrieving albums from the catalogue.</summary>
    IAlbumsClient Albums { get; }

    /// <summary>Endpoints for retrieving artists from the catalogue.</summary>
    IArtistsClient Artists { get; }

    /// <summary>Endpoints for retrieving playlists.</summary>
    IPlaylistsClient Playlists { get; }

    /// <summary>Endpoints for the signed-in account.</summary>
    IAccountClient Account { get; }

    /// <summary>Endpoints for a user's library (likes).</summary>
    ILibraryClient Library { get; }

    /// <summary>Endpoints for the catalogue's genre tree.</summary>
    IGenresClient Genres { get; }

    /// <summary>Endpoints for retrieving music clips.</summary>
    IClipsClient Clips { get; }

    /// <summary>Endpoints for retrieving the production credits of tracks and clips.</summary>
    ICreditsClient Credits { get; }

    /// <summary>Endpoints for retrieving the legal disclaimers attached to catalogue entities.</summary>
    IDisclaimersClient Disclaimers { get; }

    /// <summary>Endpoints for retrieving record labels and their catalogue.</summary>
    ILabelsClient Labels { get; }

    /// <summary>Endpoints for the personalized landing page and feed.</summary>
    ILandingClient Landing { get; }

    /// <summary>Endpoints for radio (rotor) stations.</summary>
    IRadioClient Radio { get; }

    /// <summary>Endpoints for concerts and events.</summary>
    IConcertsClient Concerts { get; }

    /// <summary>Endpoints for meta-tag (curated collection) pages.</summary>
    IMetatagsClient Metatags { get; }

    /// <summary>Endpoints for cross-device playback queues.</summary>
    IQueueClient Queue { get; }

    /// <summary>Endpoints for the user's pinned entities.</summary>
    IPinsClient Pins { get; }

    /// <summary>Endpoints for the user's pre-saved (upcoming) releases.</summary>
    IPresavesClient Presaves { get; }

    /// <summary>Endpoints for the user's listening history.</summary>
    IMusicHistoryClient MusicHistory { get; }
}
