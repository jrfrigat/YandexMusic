using System.Text.Json.Serialization;
using YandexMusic.Authentication;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Account;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Clips;
using YandexMusic.Models.Concerts;
using YandexMusic.Models.Credits;
using YandexMusic.Models.Disclaimers;
using YandexMusic.Models.Labels;
using YandexMusic.Models.Landing;
using YandexMusic.Models.Library;
using YandexMusic.Models.Metatags;
using YandexMusic.Models.Pins;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Presaves;
using YandexMusic.Models.Queue;
using YandexMusic.Models.Radio;
using YandexMusic.Models.Search;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Serialization;

/// <summary>
/// The System.Text.Json source-generation context for the library. Every type that crosses the
/// wire is registered here so that serialization uses compile-time metadata instead of runtime
/// reflection, keeping the hot path allocation-friendly and trim/AOT-safe.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ErrorEnvelope))]
[JsonSerializable(typeof(ApiResponse<List<Track>>))]
[JsonSerializable(typeof(ApiResponse<List<DownloadInfo>>))]
[JsonSerializable(typeof(ApiResponse<TrackSupplement>))]
[JsonSerializable(typeof(ApiResponse<SimilarTracks>))]
[JsonSerializable(typeof(ApiResponse<SearchResult>))]
[JsonSerializable(typeof(ApiResponse<SearchSuggestions>))]
[JsonSerializable(typeof(ApiResponse<Album>))]
[JsonSerializable(typeof(ApiResponse<List<Album>>))]
[JsonSerializable(typeof(ApiResponse<ArtistBriefInfo>))]
[JsonSerializable(typeof(ApiResponse<ArtistTracksPage>))]
[JsonSerializable(typeof(ApiResponse<ArtistAlbumsPage>))]
[JsonSerializable(typeof(ApiResponse<Playlist>))]
[JsonSerializable(typeof(ApiResponse<List<Playlist>>))]
[JsonSerializable(typeof(ApiResponse<AccountStatus>))]
[JsonSerializable(typeof(ApiResponse<LikedTracksEnvelope>))]
[JsonSerializable(typeof(ApiResponse<List<LikedAlbum>>))]
[JsonSerializable(typeof(ApiResponse<List<Artist>>))]
[JsonSerializable(typeof(ApiResponse<List<Genre>>))]
[JsonSerializable(typeof(ApiResponse<TrackLyrics>))]
[JsonSerializable(typeof(ApiResponse<TrackFullInfo>))]
[JsonSerializable(typeof(ApiResponse<TrackTrailer>))]
[JsonSerializable(typeof(ApiResponse<AfterTrackResponse>))]
[JsonSerializable(typeof(ApiResponse<string>))]
[JsonSerializable(typeof(ApiResponse<Label>))]
[JsonSerializable(typeof(ApiResponse<LabelAlbums>))]
[JsonSerializable(typeof(ApiResponse<LabelArtists>))]
[JsonSerializable(typeof(ApiResponse<Disclaimer>))]
[JsonSerializable(typeof(ApiResponse<Credits>))]
[JsonSerializable(typeof(ApiResponse<List<Clip>>))]
[JsonSerializable(typeof(ApiResponse<ClipsWillLike>))]
[JsonSerializable(typeof(ApiResponse<List<string>>))]
// Landing.
[JsonSerializable(typeof(ApiResponse<Feed>))]
[JsonSerializable(typeof(ApiResponse<WizardStatus>))]
[JsonSerializable(typeof(ApiResponse<Landing>))]
[JsonSerializable(typeof(ApiResponse<ChartInfo>))]
[JsonSerializable(typeof(ApiResponse<LandingList>))]
[JsonSerializable(typeof(ApiResponse<TagResult>))]
// Radio (rotor).
[JsonSerializable(typeof(ApiResponse<Dashboard>))]
[JsonSerializable(typeof(ApiResponse<List<StationResult>>))]
[JsonSerializable(typeof(ApiResponse<StationTracksResult>))]
// Concerts.
[JsonSerializable(typeof(ApiResponse<ArtistConcerts>))]
[JsonSerializable(typeof(ApiResponse<ConcertInfo>))]
[JsonSerializable(typeof(ApiResponse<ConcertSkeleton>))]
[JsonSerializable(typeof(ApiResponse<ConcertFeed>))]
[JsonSerializable(typeof(ApiResponse<ConcertLocations>))]
[JsonSerializable(typeof(ApiResponse<ConcertTabConfig>))]
// Metatags.
[JsonSerializable(typeof(ApiResponse<Metatags>))]
[JsonSerializable(typeof(ApiResponse<Metatag>))]
[JsonSerializable(typeof(ApiResponse<MetatagAlbums>))]
[JsonSerializable(typeof(ApiResponse<MetatagArtists>))]
[JsonSerializable(typeof(ApiResponse<MetatagPlaylists>))]
// Artists (extended).
[JsonSerializable(typeof(ApiResponse<ArtistSimilar>))]
[JsonSerializable(typeof(ApiResponse<ArtistLinks>))]
[JsonSerializable(typeof(ApiResponse<ArtistAbout>))]
[JsonSerializable(typeof(ApiResponse<ArtistClips>))]
[JsonSerializable(typeof(ApiResponse<ArtistDonations>))]
[JsonSerializable(typeof(ApiResponse<ArtistInfo>))]
[JsonSerializable(typeof(ApiResponse<ArtistSkeleton>))]
[JsonSerializable(typeof(ApiResponse<ArtistTrailer>))]
// Albums (extended).
[JsonSerializable(typeof(ApiResponse<AlbumSimilarEntities>))]
[JsonSerializable(typeof(ApiResponse<AlbumTrailer>))]
// Account (extended).
[JsonSerializable(typeof(ApiResponse<UserSettings>))]
[JsonSerializable(typeof(ApiResponse<Settings>))]
[JsonSerializable(typeof(ApiResponse<PermissionAlerts>))]
[JsonSerializable(typeof(ApiResponse<Dictionary<string, string>>))]
[JsonSerializable(typeof(ApiResponse<Dictionary<string, ExperimentDetail>>))]
[JsonSerializable(typeof(ApiResponse<PromoCodeStatus>))]
// Playlists (extended).
[JsonSerializable(typeof(ApiResponse<UserSettingsResponse>))]
[JsonSerializable(typeof(ApiResponse<PlaylistRecommendations>))]
[JsonSerializable(typeof(ApiResponse<PlaylistSimilarEntities>))]
[JsonSerializable(typeof(ApiResponse<PlaylistsList>))]
[JsonSerializable(typeof(ApiResponse<GeneratedPlaylist>))]
[JsonSerializable(typeof(ApiResponse<PlaylistTrailer>))]
[JsonSerializable(typeof(List<PlaylistDiffOperation>))]
// Library (likes/dislikes).
[JsonSerializable(typeof(ApiResponse<List<Like>>))]
[JsonSerializable(typeof(ApiResponse<LibraryRevisionResult>))]
[JsonSerializable(typeof(ApiResponse<System.Text.Json.JsonElement>))]
// Queue.
[JsonSerializable(typeof(ApiResponse<QueuesListResult>))]
[JsonSerializable(typeof(ApiResponse<YandexMusic.Models.Queue.Queue>))]
[JsonSerializable(typeof(ApiResponse<QueueUpdateResult>))]
[JsonSerializable(typeof(ApiResponse<QueueCreateResult>))]
[JsonSerializable(typeof(YandexMusic.Models.Queue.Queue))]
// Pins.
[JsonSerializable(typeof(ApiResponse<PinsList>))]
[JsonSerializable(typeof(ApiResponse<Pin>))]
// Presaves.
[JsonSerializable(typeof(ApiResponse<Presaves>))]
// MusicHistory.
[JsonSerializable(typeof(ApiResponse<YandexMusic.Models.MusicHistory.MusicHistory>))]
[JsonSerializable(typeof(ApiResponse<YandexMusic.Models.MusicHistory.MusicHistoryItems>))]
[JsonSerializable(typeof(Track))]
[JsonSerializable(typeof(Album))]
[JsonSerializable(typeof(Artist))]
[JsonSerializable(typeof(Playlist))]
// Authentication (bare OAuth JSON, not enveloped).
[JsonSerializable(typeof(DeviceCode))]
[JsonSerializable(typeof(OAuthToken))]
[JsonSerializable(typeof(OAuthErrorResponse))]
internal sealed partial class YandexMusicJsonContext : JsonSerializerContext;
