using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Concerts;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Concerts domain models.</summary>
public sealed class ConcertsTests
{
    [Fact]
    public void Deserializes_ArtistConcerts()
    {
        const string json =
            """
            { "result": {
              "artistTitle": "Some Artist",
              "concerts": [ {
                "id": "abc-123", "concertTitle": "Live Tour", "city": "Moscow", "place": "Arena",
                "datetime": "2026-07-01T19:00:00+03:00", "contentRating": "16+",
                "images": [ "img1", "img2" ],
                "minPrice": { "value": 1500, "currency": "RUB", "currencySymbol": "₽" },
                "cashback": { "title": "5%", "valuePercent": 5 },
                "eventInfo": { "type": "concert" }
              } ]
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistConcerts>>());
        var artistConcerts = response!.Result!;

        Assert.Equal("Some Artist", artistConcerts.ArtistTitle);
        var concert = Assert.Single(artistConcerts.Concerts);
        Assert.Equal("abc-123", concert.Id);
        Assert.Equal("Moscow", concert.City);
        Assert.Equal(1500, concert.MinPrice!.Value);
        Assert.Equal("₽", concert.MinPrice.CurrencySymbol);
        Assert.Equal(5, concert.Cashback!.ValuePercent);
        Assert.Equal("concert", concert.EventInfo!.Type);
        Assert.Equal(2, concert.Images!.Count);
    }

    [Fact]
    public void Deserializes_ConcertInfo()
    {
        const string json =
            """
            { "result": {
              "concert": { "id": "uuid-1", "concertTitle": "Festival" },
              "minPrice": { "value": 2000, "currency": "RUB" },
              "covers": [ { "type": "pic", "uri": "cover%%" } ],
              "description": { "text": "A great show", "source": "editorial" },
              "leadArtistId": 99887766
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ConcertInfo>>());
        var info = response!.Result!;

        Assert.Equal("uuid-1", info.Concert!.Id);
        Assert.Equal(2000, info.MinPrice!.Value);
        Assert.Single(info.Covers);
        Assert.Equal("A great show", info.Description!.Text);
        Assert.Equal("editorial", info.Description.Source);
        Assert.Equal(99887766L, info.LeadArtistId);
    }

    [Fact]
    public void Deserializes_ConcertFeedAndTabConfig()
    {
        const string feedJson =
            """
            { "result": {
              "items": [ {
                "type": "concert_item",
                "data": { "concert": { "id": "c1" }, "minPrice": { "value": 500 } }
              } ]
            } }
            """;

        var feed = JsonSerializer.Deserialize(feedJson, YandexMusicJson.TypeInfo<ApiResponse<ConcertFeed>>())!.Result!;
        var item = Assert.Single(feed.Items);
        Assert.Equal("concert_item", item.Type);
        Assert.Equal("c1", item.Data!.Concert!.Id);
        Assert.Equal(500, item.Data.MinPrice!.Value);

        const string tabJson =
            """
            { "result": { "config": { "top": { "offset": 0, "limit": 5 }, "feed": { "offset": 5, "limit": -1 } } } }
            """;

        var tabConfig = JsonSerializer.Deserialize(tabJson, YandexMusicJson.TypeInfo<ApiResponse<ConcertTabConfig>>())!.Result!;
        Assert.Equal(5, tabConfig.Config!.Top!.Limit);
        Assert.Equal(-1, tabConfig.Config.Feed!.Limit);
    }
}
