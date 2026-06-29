using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Artists;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the full artist model and the consolidated brief-info response.</summary>
public sealed class ArtistDeserializationTests
{
    private const string BriefInfoEnvelope =
        """
        {
          "result": {
            "artist": {
              "id": 79215, "name": "Queen", "various": false, "composer": false, "available": true,
              "ogImage": "x/%%",
              "counts": { "tracks": 500, "directAlbums": 30, "alsoAlbums": 100, "alsoTracks": 50 },
              "ratings": { "week": 1, "month": 2, "day": 3 },
              "links": [ { "title": "Official site", "href": "https://queen.com", "type": "official" } ],
              "likesCount": 9999, "ticketsAvailable": false
            },
            "albums": [ { "id": 3, "title": "A Night at the Opera", "metaType": "music" } ],
            "alsoAlbums": [],
            "popularTracks": [ { "id": "10", "title": "Bohemian Rhapsody", "type": "music" } ],
            "similarArtists": [ { "id": 80, "name": "David Bowie", "available": true } ]
          }
        }
        """;

    [Fact]
    public void Deserializes_BriefInfo()
    {
        var response = JsonSerializer.Deserialize(BriefInfoEnvelope, YandexMusicJson.TypeInfo<ApiResponse<ArtistBriefInfo>>());
        var info = response!.Result!;

        Assert.Equal("Queen", info.Artist.Name);
        Assert.Equal(500, info.Artist.Counts!.Tracks);
        Assert.Equal(1, info.Artist.Ratings!.Week);
        Assert.Equal("https://queen.com", info.Artist.Links![0].Href);
        Assert.Equal(9999, info.Artist.LikesCount);

        Assert.Equal("A Night at the Opera", info.Albums[0].Title);
        Assert.Equal("Bohemian Rhapsody", info.PopularTracks[0].Title);
        Assert.Equal("David Bowie", info.SimilarArtists[0].Name);
    }

    [Fact]
    public void Deserializes_ArtistEnrichments()
    {
        const string json =
            """{ "result": { "artist": { "id": 1, "name": "A", "available": true, "description": { "text": "bio", "uri": "https://x" }, "countries": [ "RU", "US" ] } } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistBriefInfo>>());
        var artist = response!.Result!.Artist;

        Assert.Equal("bio", artist.Description!.Text);
        Assert.Equal("https://x", artist.Description.Uri);
        Assert.Equal("RU", artist.Countries![0]);
    }

    [Fact]
    public void Deserializes_TracksPage()
    {
        const string json =
            """{ "result": { "pager": { "page": 0, "perPage": 5, "total": 100 }, "tracks": [ { "id": "10", "title": "Bohemian Rhapsody", "type": "music" } ] } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistTracksPage>>());
        var pageResult = response!.Result!;

        Assert.Equal(100, pageResult.Pager.Total);
        Assert.Equal("Bohemian Rhapsody", pageResult.Tracks[0].Title);
    }

    [Fact]
    public void Deserializes_DirectAlbumsPage()
    {
        const string json =
            """{ "result": { "pager": { "page": 0, "perPage": 5, "total": 30 }, "albums": [ { "id": 3, "title": "A Night at the Opera", "metaType": "music" } ] } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistAlbumsPage>>());
        var pageResult = response!.Result!;

        Assert.Equal(30, pageResult.Pager.Total);
        Assert.Equal("A Night at the Opera", pageResult.Albums[0].Title);
    }
}
