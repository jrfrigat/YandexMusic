using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Library;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the library likes/dislikes models added for the write side.</summary>
public sealed class LibraryMutationsTests
{
    [Fact]
    public void Deserializes_LikedPlaylists()
    {
        const string json =
            """
            { "result": [ {
              "timestamp": "2020-01-01T00:00:00+03:00",
              "shortDescription": "short",
              "description": "long",
              "isPremiere": true,
              "playlist": { "kind": 1, "title": "Favourites", "trackCount": 10 }
            } ] }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<Like>>>());
        var like = Assert.Single(response!.Result!);

        Assert.Equal("2020-01-01T00:00:00+03:00", like.Timestamp);
        Assert.Equal("short", like.ShortDescription);
        Assert.Equal("long", like.Description);
        Assert.True(like.IsPremiere);
        Assert.Equal("Favourites", like.Playlist!.Title);
        Assert.Null(like.Album);
        Assert.Null(like.Artist);
    }

    [Fact]
    public void Deserializes_RevisionResult()
    {
        const string json = """{ "result": { "revision": 42 } }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<LibraryRevisionResult>>());

        Assert.Equal(42, response!.Result!.Revision);
    }
}
