using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Radio;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Radio (rotor) models, including restrictions and station batches.</summary>
public sealed class RadioTests
{
    [Fact]
    public void Deserializes_Dashboard_WithStationAndRestrictions()
    {
        const string json =
            """
            { "result": {
              "dashboardId": "d1",
              "pumpkin": false,
              "stations": [ {
                "station": {
                  "id": { "type": "genre", "tag": "pop" },
                  "name": "Pop",
                  "icon": { "backgroundColor": "#fff", "imageUrl": "u" },
                  "mtsIcon": { "backgroundColor": "#000", "imageUrl": "m" },
                  "geocellIcon": { "backgroundColor": "#111", "imageUrl": "g" },
                  "idForFrom": "pop",
                  "restrictions": {
                    "language": { "type": "enum", "name": "Language",
                      "possibleValues": [ { "value": "any", "name": "Any" } ] },
                    "mood": { "type": "discrete-scale", "name": "Mood",
                      "min": { "value": "1", "name": "Sad" }, "max": { "value": "4", "name": "Fun" } }
                  },
                  "restrictions2": { "diversity": { "type": "enum", "name": "Diversity", "possibleValues": [] } },
                  "parentId": { "type": "user", "tag": "onyourwave" }
                },
                "settings2": { "language": "any", "diversity": "default", "moodEnergy": "all" },
                "adParams": { "partnerId": 42, "categoryId": "7", "pageRef": "p", "targetRef": "t", "otherParams": "o", "adVolume": -13 },
                "rupTitle": "rt", "customName": "My Pop"
              } ]
            } }
            """;

        var dashboard = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Dashboard>>())!.Result;

        Assert.NotNull(dashboard);
        Assert.Equal("d1", dashboard!.DashboardId);
        var result = Assert.Single(dashboard.Stations);
        Assert.Equal("Pop", result.Station!.Name);
        Assert.Equal("genre", result.Station.Id!.Type);
        Assert.Equal("#fff", result.Station.Icon.BackgroundColor);
        Assert.Equal("any", result.Station.Restrictions.Language!.PossibleValues[0].Value);
        Assert.Equal("4", result.Station.Restrictions.Mood!.Max!.Value);
        Assert.Equal("onyourwave", result.Station.ParentId!.Tag);
        Assert.Equal("all", result.Settings2!.MoodEnergy);
        Assert.Equal("42", result.AdParams!.PartnerId);
        Assert.Equal("My Pop", result.CustomName);
    }

    [Fact]
    public void Deserializes_StationTracks_WithBatchId()
    {
        const string json =
            """
            { "result": {
              "id": { "type": "genre", "tag": "pop" },
              "batchId": "b-123",
              "pumpkin": false,
              "sequence": [ { "type": "track", "liked": true, "track": { "id": "100", "title": "Song" } } ]
            } }
            """;

        var batch = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<StationTracksResult>>())!.Result;

        Assert.NotNull(batch);
        Assert.Equal("b-123", batch!.BatchId);
        var item = Assert.Single(batch.Sequence);
        Assert.Equal("track", item.Type);
        Assert.True(item.Liked);
        Assert.Equal("Song", item.Track!.Title);
    }
}
