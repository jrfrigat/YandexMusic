using System.Text.Json;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the custom JSON converters that absorb the API's quirks.</summary>
public sealed class ConverterTests
{
    private static JsonSerializerOptions With<TConverter>(TConverter converter)
        where TConverter : System.Text.Json.Serialization.JsonConverter
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        return options;
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("\"abc\"", "abc")]
    [InlineData("9999999999", "9999999999")]
    public void FlexibleString_ReadsNumberOrString(string json, string expected)
    {
        var options = With(new FlexibleStringConverter());
        Assert.Equal(expected, JsonSerializer.Deserialize<string>(json, options));
    }

    [Fact]
    public void FlexibleString_ReadsNull()
    {
        var options = With(new FlexibleStringConverter());
        Assert.Null(JsonSerializer.Deserialize<string>("null", options));
    }

    [Theory]
    [InlineData("\"music\"", TrackType.Music)]
    [InlineData("\"podcast-episode\"", TrackType.PodcastEpisode)]
    [InlineData("\"podcast_episode\"", TrackType.PodcastEpisode)]
    [InlineData("\"PODCAST_EPISODE\"", TrackType.PodcastEpisode)]
    public void TolerantEnum_NormalizesSeparatorsAndCase(string json, TrackType expected)
    {
        var options = With(new TolerantEnumConverter<TrackType>());
        Assert.Equal(expected, JsonSerializer.Deserialize<TrackType>(json, options));
    }

    [Theory]
    [InlineData("\"something-new-from-server\"")]
    [InlineData("null")]
    public void TolerantEnum_FallsBackToUnknown(string json)
    {
        var options = With(new TolerantEnumConverter<TrackType>());
        Assert.Equal(TrackType.Unknown, JsonSerializer.Deserialize<TrackType>(json, options));
    }

    [Fact]
    public void SearchBest_DispatchesByType()
    {
        // Routed through SearchResult (a registered type) so the source-generated metadata is used.
        const string json =
            """
            { "result": { "best": { "type": "track", "result": { "id": "1", "title": "Song" } } } }
            """;

        var result = JsonSerializer.Deserialize(
            json,
            YandexMusicJson.TypeInfo<YandexMusic.Http.ApiResponse<YandexMusic.Models.Search.SearchResult>>())!.Result;

        var best = result!.Best;
        Assert.NotNull(best);
        Assert.Equal(YandexMusic.Models.Search.SearchResultType.Track, best!.Type);
        Assert.Equal("Song", best.Track!.Title);
    }
}
