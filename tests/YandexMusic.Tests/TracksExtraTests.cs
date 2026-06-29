using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the lyrics, full-info, trailer and after-track models added to the tracks domain.</summary>
public sealed class TracksExtraTests
{
    [Fact]
    public void Deserializes_TrackLyrics_WithMajor()
    {
        const string json =
            """
            { "result": { "downloadUrl": "https://ya.ru/lyrics.txt", "lyricId": 42, "externalLyricId": "ext-7",
              "writers": ["A", "B"], "major": { "id": 1, "name": "lyricfind", "prettyName": "LyricFind" } } }
            """;

        var lyrics = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<TrackLyrics>>())!.Result;

        Assert.NotNull(lyrics);
        Assert.Equal("https://ya.ru/lyrics.txt", lyrics!.DownloadUrl);
        Assert.Equal(42, lyrics.LyricId);
        Assert.Equal("ext-7", lyrics.ExternalLyricId);
        Assert.Equal(["A", "B"], lyrics.Writers!);
        Assert.Equal("LyricFind", lyrics.Major!.PrettyName);
    }

    [Fact]
    public void Deserializes_TrackFullInfo_WithRelatedTracks()
    {
        const string json =
            """
            { "result": { "track": { "id": "1", "title": "T" }, "similarTracks": [ { "id": "2" } ],
              "alsoInAlbums": [ { "id": "3" } ], "aliases": ["x"], "artists": [ { "id": 9, "name": "Ar" } ] } }
            """;

        var info = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<TrackFullInfo>>())!.Result;

        Assert.NotNull(info);
        Assert.Equal("1", info!.Track!.Id);
        Assert.Equal("2", Assert.Single(info.SimilarTracks!).Id);
        Assert.Equal("3", Assert.Single(info.AlsoInAlbums!).Id);
        Assert.Equal("Ar", Assert.Single(info.Artists!).Name);
    }

    [Fact]
    public void Deserializes_AfterTrack_UnwrapsShotEvent()
    {
        const string json =
            """
            { "result": { "shotEvent": { "eventId": "e1", "shots": [ { "order": 0, "played": false,
              "shotId": "s1", "status": "ready", "shotData": { "coverUri": "c", "mdsUrl": "m", "shotText": "hi",
              "shotType": { "id": "alice", "title": "Alice" } } } ] } } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<AfterTrackResponse>>())!.Result;
        var shotEvent = response!.ShotEvent;

        Assert.NotNull(shotEvent);
        Assert.Equal("e1", shotEvent!.EventId);
        var shot = Assert.Single(shotEvent.Shots!);
        Assert.Equal("s1", shot.ShotId);
        Assert.Equal("hi", shot.ShotData!.ShotText);
        Assert.Equal("Alice", shot.ShotData.ShotType!.Title);
    }
}
