using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.MusicHistory;
using YandexMusic.Serialization;
using Xunit;
using MusicHistoryModel = YandexMusic.Models.MusicHistory.MusicHistory;

namespace YandexMusic.Tests;

/// <summary>Verifies the MusicHistory domain models.</summary>
public sealed class MusicHistoryTests
{
    [Fact]
    public void Deserializes_MusicHistory_DayTabs()
    {
        const string json =
            """
            { "result": {
              "history_tabs": [ {
                "date": "2026-06-29",
                "items": [ {
                  "context": { "type": "album", "data": { "item_id": { "id": 42 } } },
                  "tracks": [
                    { "type": "track", "data": { "item_id": { "track_id": 1001, "album_id": 42 } } }
                  ]
                } ]
              } ]
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<MusicHistoryModel>>());
        var history = response!.Result!;

        var tab = Assert.Single(history.HistoryTabs!);
        Assert.Equal("2026-06-29", tab.Date);

        var group = Assert.Single(tab.Items!);
        Assert.Equal("album", group.Context!.Type);
        Assert.Equal("42", group.Context.Data!.ItemId!.Id);

        var track = Assert.Single(group.Tracks!);
        Assert.Equal("track", track.Type);
        Assert.Equal("1001", track.Data!.ItemId!.TrackId);
        Assert.Equal("42", track.Data.ItemId.AlbumId);
    }

    [Fact]
    public void Deserializes_MusicHistoryItems_WaveContext()
    {
        const string json =
            """
            { "result": {
              "items": [ {
                "type": "wave",
                "data": {
                  "item_id": { "seeds": [ "album:12345" ] },
                  "full_model": {
                    "context": {
                      "tracks_count": 7,
                      "simple_wave_foreground_image_url": "fg.png",
                      "wave": { "name": "My Wave", "seeds": [ "album:12345" ] }
                    }
                  }
                }
              } ]
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<MusicHistoryItems>>());
        var items = response!.Result!;

        var item = Assert.Single(items.Items!);
        Assert.Equal("wave", item.Type);
        Assert.Equal("album:12345", Assert.Single(item.Data!.ItemId!.Seeds!));

        var context = item.Data.FullModel!.Context!;
        Assert.Equal(7, context.TracksCount);
        Assert.Equal("fg.png", context.SimpleWaveForegroundImageUrl);
        Assert.Equal("My Wave", context.Wave!.Name);
    }
}
