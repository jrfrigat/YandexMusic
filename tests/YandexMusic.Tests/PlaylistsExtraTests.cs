using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Account;
using YandexMusic.Models.Playlists;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the additional playlist models (settings, recommendations, similar entities, trailers and lists).</summary>
public sealed class PlaylistsExtraTests
{
    [Fact]
    public void Deserializes_UserSettingsResponse()
    {
        const string json =
            """
            {
              "result": {
                "userSettings": {
                  "uid": 123, "lastFmScrobblingEnabled": false, "shuffleEnabled": true,
                  "volumePercents": 80, "modified": "2021-01-01T00:00:00+03:00",
                  "userMusicVisibility": "public", "theme": "black",
                  "adsDisabled": true, "diskEnabled": false
                }
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<UserSettingsResponse>>());
        UserSettings settings = response!.Result!.UserSettings!;

        Assert.Equal(123, settings.Uid);
        Assert.True(settings.ShuffleEnabled);
        Assert.Equal(80, settings.VolumePercents);
        Assert.Equal("black", settings.Theme);
        Assert.True(settings.AdsDisabled);
        Assert.False(settings.DiskEnabled);
    }

    [Fact]
    public void Deserializes_PlaylistRecommendations()
    {
        const string json =
            """
            {
              "result": {
                "batchId": "batch-7",
                "tracks": [ { "id": "10", "title": "Song", "type": "music", "durationMs": 200000 } ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PlaylistRecommendations>>());
        var recommendations = response!.Result!;

        Assert.Equal("batch-7", recommendations.BatchId);
        var track = Assert.Single(recommendations.Tracks);
        Assert.Equal("Song", track.Title);
    }

    [Fact]
    public void Deserializes_PlaylistSimilarEntities_WithWaveAgent()
    {
        const string json =
            """
            {
              "result": {
                "items": [
                  {
                    "type": "wave_agent_item",
                    "data": {
                      "wave": { "name": "Mix", "seeds": [ "album:12345" ] },
                      "agent": { "animation_uri": "anim://x", "entity": { "type": "album" } }
                    }
                  }
                ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PlaylistSimilarEntities>>());
        var item = Assert.Single(response!.Result!.Items!);

        Assert.Equal("wave_agent_item", item.Type);
        Assert.Equal("Mix", item.Data!.Wave!.Name);
        Assert.Equal("album:12345", Assert.Single(item.Data.Wave.Seeds!));
        Assert.Equal("anim://x", item.Data.Agent!.AnimationUri);
        Assert.Equal("album", item.Data.Agent.Entity!.Type);
    }

    [Fact]
    public void Deserializes_PlaylistsList()
    {
        const string json =
            """
            {
              "result": {
                "playlists": [
                  { "title": "First", "kind": 100, "uid": 5 },
                  { "title": "Second", "kind": 200, "uid": 5 }
                ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PlaylistsList>>());
        var playlists = response!.Result!.Playlists!;

        Assert.Equal(2, playlists.Count);
        Assert.Equal("First", playlists[0].Title);
        Assert.Equal(200, playlists[1].Kind);
    }

    [Fact]
    public void Deserializes_PlaylistTrailer()
    {
        const string json =
            """
            {
              "result": {
                "shareable": true,
                "playlist": { "title": "Trailered", "kind": 9, "uid": 1 },
                "trailer": { "title": "Preview", "tracks": [ { "id": "1", "title": "T", "type": "music", "durationMs": 1000 } ] }
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PlaylistTrailer>>());
        var trailer = response!.Result!;

        Assert.True(trailer.Shareable);
        Assert.Equal("Trailered", trailer.Playlist!.Title);
        Assert.Equal("Preview", trailer.Trailer!.Title);
        Assert.Single(trailer.Trailer.Tracks!);
    }
}
