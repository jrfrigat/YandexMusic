using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Search;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Regression tests for live-API response shapes that differ from the obvious model (found by running
/// the sample CLI against the real service): the same fields can arrive as numbers in one endpoint and
/// strings in another, and album labels can be bare strings in search results.
/// </summary>
public sealed class LiveShapeRegressionTests
{
    [Theory]
    [InlineData("166", 166L)]      // JSON number
    [InlineData("\"166\"", 166L)]  // JSON string (search sends exec-duration-millis like this)
    [InlineData("\"abc\"", null)]  // unparseable string -> null, never throws
    [InlineData("null", null)]
    public void FlexibleInt64_ReadsStringOrNumber(string json, long? expected)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FlexibleInt64Converter());
        Assert.Equal(expected, JsonSerializer.Deserialize<long?>(json, options));
    }

    [Fact]
    public void Search_DeserializesLiveShapes_NumericIdsStringLabelsStringDuration()
    {
        // Mirrors the awkward bits of a real /search response: a string exec-duration-millis, numeric
        // track/album/artist ids, and album labels as bare strings.
        const string json =
            """
            {
              "invocationInfo": { "exec-duration-millis": "166" },
              "result": {
                "tracks": { "results": [ { "id": 42, "title": "Song", "artists": [ { "id": 7, "name": "Artist" } ] } ] },
                "albums": { "results": [ { "id": 3, "title": "Album", "labels": [ "Some Label" ] } ] },
                "artists": { "results": [ { "id": 9, "name": "Solo" } ] }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>());

        Assert.Equal(166L, envelope!.InvocationInfo!.ExecDurationMillis);
        var result = envelope.Result!;

        var track = Assert.Single(result.Tracks!.Results);
        Assert.Equal("42", track.Id);
        Assert.Equal("7", Assert.Single(track.Artists).Id);

        var album = Assert.Single(result.Albums!.Results);
        Assert.Equal("3", album.Id);
        Assert.Equal("Some Label", Assert.Single(album.Labels!).Name);

        Assert.Equal("9", Assert.Single(result.Artists!.Results).Id);
    }

    [Fact]
    public void Album_Labels_StillReadObjectForm()
    {
        // The non-search endpoints return labels as objects; that path must keep working.
        const string json =
            """
            { "result": { "id": 3, "title": "Album", "labels": [ { "id": 5, "name": "Object Label" } ] } }
            """;

        var album = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<YandexMusic.Models.Albums.Album>>())!.Result!;

        var label = Assert.Single(album.Labels!);
        Assert.Equal("Object Label", label.Name);
        Assert.Equal(5, label.Id);
    }
}
