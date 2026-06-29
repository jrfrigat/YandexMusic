using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies that the track models deserialize a real (public) Yandex Music API envelope correctly,
/// including camelCase mapping, tolerant enum handling (kebab-case and UPPER_SNAKE), nested objects
/// and date parsing.
/// </summary>
public sealed class TrackDeserializationTests
{
    private const string TrackEnvelope =
        """
        {
          "invocationInfo": { "req-id": "x", "hostname": "h", "exec-duration-millis": 8 },
          "result": [{
            "id": "4", "realId": "4", "title": "Just In Time", "type": "music",
            "available": true, "availableForPremiumUsers": true,
            "durationMs": 315060, "previewDurationMs": 30000,
            "trackSharingFlag": "COVER_ONLY", "trackSource": "OWN",
            "coverUri": "avatars.yandex.net/x/%%",
            "r128": { "i": -13.3, "tp": 0.06 },
            "fade": { "inStart": 0.2, "inStop": 1.5, "outStart": 303.8, "outStop": 308.4 },
            "lyricsInfo": { "hasAvailableSyncLyrics": false, "hasAvailableTextLyrics": true },
            "artists": [{ "id": 16, "name": "Elizabeth & the Catapult",
              "cover": { "type": "from-album-cover", "uri": "u", "prefix": "p" } }],
            "albums": [{ "id": 3, "title": "Taller Children", "metaType": "music", "year": 2009,
              "releaseDate": "2009-06-09T00:00:00+04:00", "trackPosition": { "volume": 1, "index": 1 } }]
          }]
        }
        """;

    [Fact]
    public void Deserializes_RealTrackEnvelope()
    {
        var response = JsonSerializer.Deserialize(TrackEnvelope, YandexMusicJson.TypeInfo<ApiResponse<List<Track>>>());

        Assert.NotNull(response);
        Assert.Null(response.Error);
        var track = Assert.Single(response.Result!);

        Assert.Equal("Just In Time", track.Title);
        Assert.Equal(TrackType.Music, track.Type);
        Assert.Equal(315060, track.DurationMs);
        Assert.True(track.AvailableForPremiumUsers);
        Assert.Equal(-13.3, track.R128!.I, 3);
        Assert.True(track.LyricsInfo!.HasAvailableTextLyrics);
    }

    [Fact]
    public void MapsTolerantEnums_KebabAndUpperSnake()
    {
        var response = JsonSerializer.Deserialize(TrackEnvelope, YandexMusicJson.TypeInfo<ApiResponse<List<Track>>>());
        var track = response!.Result![0];

        Assert.Equal(TrackSharingFlag.CoverOnly, track.TrackSharingFlag);   // "COVER_ONLY"
        Assert.Equal(TrackSource.Own, track.TrackSource);                   // "OWN"
        Assert.Equal(CoverType.FromAlbumCover, track.Artists[0].Cover!.Type); // "from-album-cover"
        Assert.Equal(AlbumMetaType.Music, track.Albums[0].MetaType);        // "music"
    }

    [Fact]
    public void MapsNestedAlbum_WithDateAndPosition()
    {
        var response = JsonSerializer.Deserialize(TrackEnvelope, YandexMusicJson.TypeInfo<ApiResponse<List<Track>>>());
        var album = response!.Result![0].Albums[0];

        Assert.Equal("Taller Children", album.Title);
        Assert.Equal(2009, album.Year);
        Assert.Equal(2009, album.ReleaseDate!.Value.Year);
        Assert.Equal(1, album.TrackPosition!.Volume);
        Assert.Equal(1, album.TrackPosition.Index);
    }

    [Fact]
    public void UnknownEnumValue_FallsBackToUnknown()
    {
        const string json = """{ "result": [{ "id": "1", "title": "t", "trackSharingFlag": "SOME_NEW_FLAG" }] }""";
        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<Track>>>());

        Assert.Equal(TrackSharingFlag.Unknown, response!.Result![0].TrackSharingFlag);
    }

    [Fact]
    public void Deserializes_TrackEnrichments_WithSubstituted()
    {
        const string json =
            """
            { "result": [ {
              "id": "1", "title": "Original", "type": "music", "version": "Remix",
              "contentWarning": "explicit", "best": true,
              "substituted": { "id": "2", "title": "Replacement", "type": "music" }
            } ] }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<Track>>>());
        var track = response!.Result![0];

        Assert.Equal("Remix", track.Version);
        Assert.Equal("explicit", track.ContentWarning);
        Assert.True(track.Best);
        Assert.Equal("Replacement", track.Substituted!.Title);
    }

    [Fact]
    public void Deserializes_Supplement_WithLyrics()
    {
        const string json =
            """{ "result": { "id": 4, "lyrics": { "id": 100, "lyrics": "line1\nline2", "fullLyrics": "l1\nl2\nl3", "hasRights": true, "textLanguage": "en", "showTranslation": false } } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<TrackSupplement>>());
        var lyrics = response!.Result!.Lyrics!;

        Assert.Contains("line1", lyrics.Text);
        Assert.Contains("l3", lyrics.FullText);
        Assert.True(lyrics.HasRights);
        Assert.Equal("en", lyrics.TextLanguage);
    }

    [Fact]
    public void Deserializes_SimilarTracks()
    {
        const string json =
            """{ "result": { "track": { "id": "4", "title": "Just In Time", "type": "music" }, "similarTracks": [ { "id": "10", "title": "Other", "type": "music" } ] } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SimilarTracks>>());
        var similar = response!.Result!;

        Assert.Equal("Just In Time", similar.Track!.Title);
        Assert.Equal("Other", similar.Tracks[0].Title);
    }
}
