using System.Text.Json;
using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Search;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Serialization;

/// <summary>
/// Reads the polymorphic <c>best</c> match of a search response: an object of the form
/// <c>{ "type": "...", "result": { … } }</c> whose <c>result</c> is deserialized into the typed
/// property matching <c>type</c>.
/// </summary>
internal sealed class SearchBestConverter : JsonConverter<SearchBest>
{
    /// <inheritdoc />
    public override SearchBest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var type = MapType(root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null);
        var result = root.TryGetProperty("result", out var resultElement) ? resultElement : default;

        Track? track = null;
        Album? album = null;
        Artist? artist = null;
        Playlist? playlist = null;

        if (result.ValueKind == JsonValueKind.Object)
        {
            switch (type)
            {
                case SearchResultType.Track:
                    track = result.Deserialize(YandexMusicJson.TypeInfo<Track>());
                    break;
                case SearchResultType.Album:
                    album = result.Deserialize(YandexMusicJson.TypeInfo<Album>());
                    break;
                case SearchResultType.Artist:
                    artist = result.Deserialize(YandexMusicJson.TypeInfo<Artist>());
                    break;
                case SearchResultType.Playlist:
                    playlist = result.Deserialize(YandexMusicJson.TypeInfo<Playlist>());
                    break;
                case SearchResultType.Podcast:
                    album = result.Deserialize(YandexMusicJson.TypeInfo<Album>());
                    break;
                case SearchResultType.PodcastEpisode:
                    track = result.Deserialize(YandexMusicJson.TypeInfo<Track>());
                    break;
                default:
                    break;
            }
        }

        return new SearchBest { Type = type, Track = track, Album = album, Artist = artist, Playlist = playlist };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SearchBest value, JsonSerializerOptions options)
        => throw new NotSupportedException("SearchBest is a read-only response model.");

    private static SearchResultType MapType(string? type) => type switch
    {
        "track" => SearchResultType.Track,
        "album" => SearchResultType.Album,
        "artist" => SearchResultType.Artist,
        "playlist" => SearchResultType.Playlist,
        "podcast" => SearchResultType.Podcast,
        "podcast_episode" => SearchResultType.PodcastEpisode,
        "video" => SearchResultType.Video,
        "user" => SearchResultType.User,
        _ => SearchResultType.Unknown,
    };
}
