using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Albums;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the full album model, including labels, the structured cover and the "with tracks"
/// volumes/pager.
/// </summary>
public sealed class AlbumDeserializationTests
{
    private const string AlbumEnvelope =
        """
        {
          "result": {
            "id": 3, "title": "Taller Children", "metaType": "music", "year": 2009,
            "releaseDate": "2009-06-09T00:00:00+04:00",
            "coverUri": "avatars.yandex.net/x/%%", "cover": { "type": "pic", "uri": "u", "prefix": "p" },
            "genre": "indie", "metaTagId": "tag", "trackCount": 12, "likesCount": 100,
            "labels": [ { "id": 123, "name": "Verve Forecast" } ],
            "hasTrailer": false,
            "artists": [ { "id": 16, "name": "Elizabeth & the Catapult", "available": true } ],
            "available": true, "availableForPremiumUsers": true,
            "sortOrder": "asc",
            "pager": { "total": 12, "page": 0, "perPage": 20 },
            "volumes": [ [ { "id": "4", "title": "Just In Time", "type": "music", "durationMs": 315060 } ] ]
          }
        }
        """;

    [Fact]
    public void Deserializes_FullAlbum()
    {
        var response = JsonSerializer.Deserialize(AlbumEnvelope, YandexMusicJson.TypeInfo<ApiResponse<Album>>());
        var album = response!.Result!;

        Assert.Equal("Taller Children", album.Title);
        Assert.Equal(2009, album.Year);
        Assert.Equal(CoverType.Pic, album.Cover!.Type);
        Assert.Equal("Verve Forecast", album.Labels![0].Name);
        Assert.Equal("tag", album.MetaTagId);
    }

    [Fact]
    public void Deserializes_WithTracks_VolumesAndPager()
    {
        var response = JsonSerializer.Deserialize(AlbumEnvelope, YandexMusicJson.TypeInfo<ApiResponse<Album>>());
        var album = response!.Result!;

        Assert.Equal(12, album.Pager!.Total);
        Assert.Equal("asc", album.SortOrder);
        Assert.Single(album.Volumes!);
        Assert.Equal("Just In Time", album.Volumes![0][0].Title);
    }
}
