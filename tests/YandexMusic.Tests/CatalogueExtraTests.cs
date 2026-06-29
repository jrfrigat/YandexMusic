using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Clips;
using YandexMusic.Models.Credits;
using YandexMusic.Models.Disclaimers;
using YandexMusic.Models.Labels;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the labels, disclaimers, credits and clips models.</summary>
public sealed class CatalogueExtraTests
{
    [Fact]
    public void Deserializes_Label_WithLinks()
    {
        const string json =
            """
            { "result": { "id": 1, "name": "Label", "description": "d", "type": "musical",
              "links": [ { "title": "Site", "href": "https://l.ru", "type": "official" } ] } }
            """;

        var label = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Label>>())!.Result;

        Assert.NotNull(label);
        Assert.Equal("Label", label!.Name);
        Assert.Equal("musical", label.Type);
        Assert.Equal("https://l.ru", Assert.Single(label.Links!).Href);
    }

    [Fact]
    public void Deserializes_LabelAlbums_WithPager()
    {
        const string json =
            """
            { "result": { "albums": [ { "id": 5, "title": "Al" } ], "pager": { "total": 1, "page": 0, "perPage": 100 } } }
            """;

        var page = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<LabelAlbums>>())!.Result;

        Assert.NotNull(page);
        Assert.Equal("Al", Assert.Single(page!.Albums).Title);
        Assert.Equal(1, page.Pager!.Total);
    }

    [Fact]
    public void Deserializes_Disclaimer_WithForeignAgent()
    {
        const string json =
            """
            { "result": { "foreignAgent": { "reason": "policy", "title": "Warning" } } }
            """;

        var disclaimer = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Disclaimer>>())!.Result;

        Assert.NotNull(disclaimer);
        Assert.Equal("policy", disclaimer!.ForeignAgent!.Reason);
        Assert.Equal("Warning", disclaimer.ForeignAgent.Title);
    }

    [Fact]
    public void Deserializes_Credits_RenamesCreditsToItems()
    {
        const string json =
            """
            { "result": { "credits": [ { "title": "composer", "value": "Bach" } ] } }
            """;

        var credits = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Credits>>())!.Result;

        Assert.NotNull(credits);
        var credit = Assert.Single(credits!.Items!);
        Assert.Equal("composer", credit.Title);
        Assert.Equal("Bach", credit.Value);
    }

    [Fact]
    public void Deserializes_Clips_WithContentRestrictions()
    {
        const string json =
            """
            { "result": [ { "clipId": 7, "title": "Clip", "trackIds": [1, 2], "explicit": true,
              "contentRestrictions": { "available": true, "disclaimers": ["explicit"] } } ] }
            """;

        var clips = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<Clip>>>())!.Result;

        var clip = Assert.Single(clips!);
        Assert.Equal(7, clip.ClipId);
        Assert.Equal([1, 2], clip.TrackIds!);
        Assert.True(clip.Explicit);
        Assert.True(clip.ContentRestrictions!.Available);
    }

    [Fact]
    public void Deserializes_ClipsWillLike_WithPager()
    {
        const string json =
            """
            { "result": { "clips": [ { "clipId": 1 } ], "pager": { "total": 1, "page": 0, "perPage": 50 } } }
            """;

        var result = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ClipsWillLike>>())!.Result;

        Assert.NotNull(result);
        Assert.Equal(1, Assert.Single(result!.Clips!).ClipId);
        Assert.Equal(50, result.Pager!.PerPage);
    }
}
