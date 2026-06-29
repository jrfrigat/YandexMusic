using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Landing;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Landing domain models and their trickier wire mappings.</summary>
public sealed class LandingTests
{
    [Fact]
    public void Deserializes_Feed_WithRenamedDayKey()
    {
        const string json =
            """
            { "result": {
              "canGetMoreEvents": true, "pumpkin": false, "isWizardPassed": true,
              "generatedPlaylists": [ { "type": "playlistOfTheDay", "ready": true, "notify": false } ],
              "headlines": [ "h1" ], "today": "2026-06-29",
              "days": [ { "day": "2026-06-28",
                "events": [ { "id": "e1", "type": "new-release", "tracksCount": 3 } ],
                "tracksToPlay": [] } ],
              "nextRevision": "2026-06-27"
            } }
            """;

        var feed = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Feed>>())!.Result;

        Assert.NotNull(feed);
        Assert.True(feed!.CanGetMoreEvents);
        Assert.True(feed.IsWizardPassed);
        Assert.Equal("playlistOfTheDay", feed.GeneratedPlaylists[0].Type);
        Assert.Equal("2026-06-28", feed.Days[0].DayDate);
        Assert.Equal("e1", feed.Days[0].Events[0].Id);
        Assert.Equal(3, feed.Days[0].Events[0].TracksCount);
        Assert.Equal("2026-06-27", feed.NextRevision);
    }

    [Fact]
    public void Deserializes_Landing_WithFlexibleContentIdAndRawBlockData()
    {
        const string json =
            """
            { "result": {
              "pumpkin": false, "contentId": 12345,
              "blocks": [ {
                "id": "b1", "type": "personal-playlists", "typeForFrom": "personal-playlists",
                "title": "For you", "entities": [ { "id": "x", "type": "personal-playlist", "data": { "type": "origin" } } ],
                "data": { "isWizardPassed": true }
              } ]
            } }
            """;

        var landing = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Landing>>())!.Result;

        Assert.NotNull(landing);
        Assert.Equal("12345", landing!.ContentId);
        var block = Assert.Single(landing.Blocks);
        Assert.Equal("personal-playlists", block.Type);
        Assert.True(block.Data!.Value.GetProperty("isWizardPassed").GetBoolean());
        Assert.Equal("personal-playlist", block.Entities[0].Type);
    }

    [Fact]
    public void Deserializes_LandingList_WithPlaylistIds()
    {
        const string json =
            """
            { "result": {
              "type": "new-playlists", "typeForFrom": "new-playlists", "title": "New",
              "newPlaylists": [ { "uid": 503646255, "kind": 1235 } ]
            } }
            """;

        var list = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<LandingList>>())!.Result;

        Assert.NotNull(list);
        Assert.Equal("new-playlists", list!.Type);
        Assert.Equal(503646255L, list.NewPlaylists[0].Uid);
        Assert.Equal(1235L, list.NewPlaylists[0].Kind);
    }

    [Fact]
    public void Deserializes_TagResult_WithNestedTag()
    {
        const string json =
            """
            { "result": {
              "tag": { "id": "belarus", "value": "belarus", "name": "Belarus", "ogDescription": "d" },
              "ids": [ { "uid": 1, "kind": 2 } ]
            } }
            """;

        var result = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<TagResult>>())!.Result;

        Assert.NotNull(result);
        Assert.Equal("belarus", result!.Tag!.Value);
        Assert.Equal("Belarus", result.Tag.Name);
        Assert.Equal(2L, result.Ids[0].Kind);
    }

    [Fact]
    public void Deserializes_WizardStatus()
    {
        const string json = """ { "result": { "isWizardPassed": true } } """;

        var status = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<WizardStatus>>())!.Result;

        Assert.NotNull(status);
        Assert.True(status!.IsWizardPassed);
    }
}
