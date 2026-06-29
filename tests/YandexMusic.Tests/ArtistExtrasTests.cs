using System.Collections.Generic;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Artists;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the additional artist domain models (similar, links, about, info, clips, donations, skeleton, trailer).</summary>
public sealed class ArtistExtrasTests
{
    [Fact]
    public void Deserializes_Similar()
    {
        const string json =
            """{ "result": { "artist": { "id": 1, "name": "A", "available": true }, "similarArtists": [ { "id": 2, "name": "B", "available": true } ] } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistSimilar>>());
        var similar = response!.Result!;

        Assert.Equal("A", similar.Artist!.Name);
        Assert.Equal("B", similar.SimilarArtists![0].Name);
    }

    [Fact]
    public void Deserializes_About()
    {
        const string json =
            """
            {
              "result": {
                "artist": { "id": 1, "name": "A", "available": true },
                "stats": { "lastMonthListeners": 1000, "lastMonthListenersDelta": -5 },
                "description": "bio",
                "links": [ { "title": "Site", "href": "https://x", "type": "official" } ],
                "covers": [ { "type": "pic", "uri": "y/%%" } ],
                "artistType": "performer"
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistAbout>>());
        var about = response!.Result!;

        Assert.Equal(1000, about.Stats!.LastMonthListeners);
        Assert.Equal(-5, about.Stats.LastMonthListenersDelta);
        Assert.Equal("bio", about.Description);
        Assert.Equal("Site", about.Links![0].Title);
        Assert.Equal("y/%%", about.Covers![0].Uri);
        Assert.Equal("performer", about.ArtistType);
    }

    [Fact]
    public void Deserializes_Info()
    {
        const string json =
            """
            {
              "result": {
                "artist": { "id": 1, "name": "A", "available": true },
                "likesCount": 42,
                "stats": { "lastMonthListeners": 7, "lastMonthListenersDelta": 1 },
                "trailer": { "available": true },
                "description": "d"
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistInfo>>());
        var info = response!.Result!;

        Assert.Equal(42, info.LikesCount);
        Assert.Equal(7, info.Stats!.LastMonthListeners);
        Assert.True(info.Trailer!.Available);
    }

    [Fact]
    public void Deserializes_Clips()
    {
        const string json =
            """
            {
              "result": {
                "items": [ { "type": "clip", "data": { "clip": { "clipId": 9, "title": "C", "trackIds": [ 1, 2 ] }, "artists": [ { "id": 1, "name": "A", "available": true } ] } } ],
                "pager": { "page": 0, "perPage": 10, "total": 1 }
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistClips>>());
        var clips = response!.Result!;

        Assert.Equal("clip", clips.Items![0].Type);
        Assert.Equal(9, clips.Items[0].Data!.Clip!.ClipId);
        Assert.Equal("A", clips.Items[0].Data!.Artists![0].Name);
        Assert.Equal(1, clips.Pager!.Total);
    }

    [Fact]
    public void Deserializes_Donations()
    {
        const string json =
            """
            {
              "result": {
                "donations": [ { "type": "donation_item", "data": { "tipUrl": "https://tip", "artist": { "id": 1, "name": "A", "available": true }, "goal": { "title": "G" } } } ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistDonations>>());
        var data = response!.Result!.Donations![0].Data!;

        Assert.Equal("https://tip", data.TipUrl);
        Assert.Equal("A", data.Artist!.Name);
        Assert.Equal("G", data.Goal!.Title);
    }

    [Fact]
    public void Deserializes_Skeleton()
    {
        const string json =
            """
            {
              "result": {
                "id": "web-artist-default",
                "title": "Artist",
                "blocks": [ { "id": "b1", "type": "TABS", "data": { "tabs": [ { "id": "t1", "title": "About", "blocks": [] } ], "selectedTabIndex": 0 } } ]
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistSkeleton>>());
        var skeleton = response!.Result!;

        Assert.Equal("web-artist-default", skeleton.Id);
        Assert.Equal("TABS", skeleton.Blocks![0].Type);
        Assert.Equal("About", skeleton.Blocks[0].Data!.Tabs![0].Title);
    }

    [Fact]
    public void Deserializes_Trailer()
    {
        const string json =
            """
            {
              "result": {
                "artist": { "id": 1, "name": "A", "available": true },
                "trailer": { "title": "T", "tracks": [ { "id": "10", "title": "Track", "type": "music" } ] }
              }
            }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistTrailer>>());
        var trailer = response!.Result!;

        Assert.Equal("A", trailer.Artist!.Name);
        Assert.Equal("T", trailer.Trailer!.Title);
        Assert.Equal("Track", trailer.Trailer.Tracks![0].Title);
    }

    [Fact]
    public void Deserializes_Links_And_TrackIds()
    {
        const string linksJson =
            """{ "result": { "links": [ { "title": "Site", "href": "https://x", "type": "social" } ] } }""";
        var links = JsonSerializer.Deserialize(linksJson, YandexMusicJson.TypeInfo<ApiResponse<ArtistLinks>>())!.Result!;
        Assert.Equal("Site", links.Links![0].Title);

        const string trackIdsJson = """{ "result": [ "1", "2", "3" ] }""";
        var ids = JsonSerializer.Deserialize(trackIdsJson, YandexMusicJson.TypeInfo<ApiResponse<List<string>>>())!.Result!;
        Assert.Equal(3, ids.Count);
        Assert.Equal("2", ids[1]);
    }
}
