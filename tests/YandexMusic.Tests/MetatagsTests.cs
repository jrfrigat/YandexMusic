using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Metatags;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the metatag models (the tree, the landing page and the paged listings).</summary>
public sealed class MetatagsTests
{
    [Fact]
    public void Deserializes_MetatagTree()
    {
        const string json =
            """
            { "result": { "trees": [ {
              "title": "Настроения и жанры", "navigationId": "moods",
              "leaves": [ { "tag": "sad", "title": "Грусть", "leaves": [ { "tag": "sad-soft", "title": "Тихая грусть" } ] } ]
            } ] } }
            """;

        var tree = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Metatags>>())!.Result;
        Assert.NotNull(tree);

        var branch = Assert.Single(tree!.Trees);
        Assert.Equal("moods", branch.NavigationId);
        var leaf = Assert.Single(branch.Leaves);
        Assert.Equal("sad", leaf.Tag);
        Assert.Equal("sad-soft", Assert.Single(leaf.Leaves).Tag);
    }

    [Fact]
    public void Deserializes_MetatagLandingPage()
    {
        const string json =
            """
            { "result": {
              "id": "sad", "coverUri": "avatars/%%", "color": "#112233",
              "title": { "title": "Грусть", "fullTitle": "Грустная музыка" },
              "liked": true, "stationId": "metatag:sad",
              "customWaveAnimationUrl": "https://example/anim.json",
              "tracksSortByValues": [ { "value": "popular", "title": "Популярные", "active": true } ]
            } }
            """;

        var metatag = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Metatag>>())!.Result;
        Assert.NotNull(metatag);
        Assert.Equal("sad", metatag!.Id);
        Assert.Equal("Грустная музыка", metatag.Title!.FullTitle);
        Assert.True(metatag.Liked);
        Assert.Equal("https://example/anim.json", metatag.CustomWaveAnimationUrl);
        Assert.Equal("popular", Assert.Single(metatag.TracksSortByValues).Value);
    }

    [Fact]
    public void Deserializes_MetatagArtistsListing()
    {
        const string json =
            """
            { "result": {
              "id": "sad", "stationId": "metatag:sad",
              "pager": { "total": 100, "page": 0, "perPage": 25 },
              "artists": [ { "artist": { "id": 1, "name": "A" }, "popularTracks": [ { "id": "10", "title": "T" } ] } ],
              "sortByValues": [ { "value": "popular", "title": "Популярные" } ]
            } }
            """;

        var listing = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<MetatagArtists>>())!.Result;
        Assert.NotNull(listing);
        Assert.Equal(100, listing!.Pager!.Total);
        var entry = Assert.Single(listing.Artists);
        Assert.Equal("A", entry.Artist!.Name);
        Assert.Equal("10", Assert.Single(entry.PopularTracks).Id);
    }
}
