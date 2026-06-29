using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Albums;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the album similar-entities and trailer models.</summary>
public sealed class AlbumsExtraTests
{
    [Fact]
    public void Deserializes_SimilarEntities()
    {
        const string json =
            """
            { "result": { "items": [ {
              "type": "wave_agent_item",
              "data": {
                "wave": { "name": "Album wave", "description": "More like this", "seeds": [ "album:12345" ] },
                "agent": { "animation_uri": "https://example/anim.json", "cover": { "type": "from-album-cover" }, "entity": { "type": "album" } }
              }
            } ] } }
            """;

        var entities = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<AlbumSimilarEntities>>())!.Result;

        Assert.NotNull(entities);
        var item = Assert.Single(entities!.Items!);
        Assert.Equal("wave_agent_item", item.Type);
        Assert.Equal("Album wave", item.Data!.Wave!.Name);
        Assert.Equal("album:12345", item.Data.Wave.Seeds![0]);
        Assert.Equal("https://example/anim.json", item.Data.Agent!.AnimationUri);
        Assert.Equal("album", item.Data.Agent.Entity!.Type);
    }

    [Fact]
    public void Deserializes_Trailer()
    {
        const string json =
            """
            { "result": {
              "album": { "id": 42, "title": "Greatest Hits" },
              "artists": [ { "id": 7, "name": "Some Artist" } ],
              "trailer": { "title": "Preview", "tracks": [ { "id": "1", "title": "Intro" } ] }
            } }
            """;

        var trailer = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<AlbumTrailer>>())!.Result;

        Assert.NotNull(trailer);
        Assert.Equal("Greatest Hits", trailer!.Album!.Title);
        Assert.Equal("Some Artist", Assert.Single(trailer.Artists!).Name);
        Assert.Equal("Preview", trailer.Trailer!.Title);
        Assert.Equal("Intro", Assert.Single(trailer.Trailer.Tracks!).Title);
    }
}
