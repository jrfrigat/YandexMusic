using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Search;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies that the search models deserialize the Yandex Music search envelope, including the
/// polymorphic <c>best</c> match and the per-category sections.
/// </summary>
public sealed class SearchDeserializationTests
{
    private const string SearchEnvelope =
        """
        {
          "result": {
            "searchRequestId": "abc",
            "text": "queen",
            "nocorrect": false,
            "misspellCorrected": false,
            "best": { "type": "artist",
              "result": { "id": 79215, "name": "Queen", "various": false, "composer": false, "available": true } },
            "artists": { "total": 12, "perPage": 20, "order": 0,
              "results": [ { "id": 79215, "name": "Queen", "available": true } ] },
            "tracks": { "total": 100, "perPage": 20, "order": 0,
              "results": [ { "id": "10", "title": "Bohemian Rhapsody", "type": "music", "durationMs": 354000 } ] }
          }
        }
        """;

    [Fact]
    public void Deserializes_SearchResult_WithSections()
    {
        var response = JsonSerializer.Deserialize(SearchEnvelope, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>());
        var result = response!.Result!;

        Assert.Equal("queen", result.Text);
        Assert.Equal("abc", result.SearchRequestId);
        Assert.Equal(12, result.Artists!.Total);
        Assert.Equal("Queen", result.Artists.Results[0].Name);
        Assert.Equal("Bohemian Rhapsody", result.Tracks!.Results[0].Title);
        Assert.Equal(354000, result.Tracks.Results[0].DurationMs);
    }

    [Fact]
    public void Deserializes_PolymorphicBest_Artist()
    {
        var response = JsonSerializer.Deserialize(SearchEnvelope, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>());
        var best = response!.Result!.Best!;

        Assert.Equal(SearchResultType.Artist, best.Type);
        Assert.NotNull(best.Artist);
        Assert.Equal("Queen", best.Artist!.Name);
        Assert.Null(best.Track);
        Assert.Null(best.Album);
    }

    [Fact]
    public void Deserializes_PodcastsAndUsersSections()
    {
        const string json =
            """
            { "result": {
              "podcasts": { "total": 5, "perPage": 20, "order": 0,
                "results": [ { "id": 9, "title": "Daily News", "metaType": "podcast" } ] },
              "podcast_episodes": { "total": 3, "perPage": 20, "order": 0,
                "results": [ { "id": "100", "title": "Episode 1", "type": "podcast-episode" } ] },
              "users": { "total": 1, "perPage": 20, "order": 0,
                "results": [ { "uid": 42, "login": "dj", "name": "DJ" } ] }
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>());
        var result = response!.Result!;

        Assert.Equal("Daily News", result.Podcasts!.Results[0].Title);
        Assert.Equal("Episode 1", result.PodcastEpisodes!.Results[0].Title);
        Assert.Equal("dj", result.Users!.Results[0].Login);
    }

    [Fact]
    public void Deserializes_Suggestions()
    {
        const string json =
            """
            { "result": {
              "best": { "type": "artist", "result": { "id": 79215, "name": "Queen", "available": true } },
              "suggestions": [ "queen", "queen bee" ]
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SearchSuggestions>>());
        var suggestions = response!.Result!;

        Assert.Equal(SearchResultType.Artist, suggestions.Best!.Type);
        Assert.Equal("Queen", suggestions.Best.Artist!.Name);
        Assert.Equal("queen", suggestions.Suggestions[0]);
    }
}
