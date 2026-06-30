using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Presaves;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Presaves models deserialize their envelopes.</summary>
public sealed class PresavesTests
{
    [Fact]
    public void Deserializes_Presaves_WithSnakeCaseKeys()
    {
        const string json =
            """
            { "result": {
              "upcoming_albums": [ { "id": 1, "title": "Upcoming" } ],
              "released_albums": [ { "id": 2, "title": "Released A" }, { "id": 3, "title": "Released B" } ]
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Presaves>>());
        var presaves = response!.Result!;

        Assert.NotNull(presaves.UpcomingAlbums);
        Assert.Single(presaves.UpcomingAlbums!);
        Assert.Equal("Upcoming", presaves.UpcomingAlbums![0].Title);

        Assert.NotNull(presaves.ReleasedAlbums);
        Assert.Equal(2, presaves.ReleasedAlbums!.Count);
        Assert.Equal("3", presaves.ReleasedAlbums![1].Id);
    }

    [Fact]
    public void Deserializes_Presaves_WithMissingLists()
    {
        const string json = """ { "result": { } } """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Presaves>>());
        var presaves = response!.Result!;

        Assert.Null(presaves.UpcomingAlbums);
        Assert.Null(presaves.ReleasedAlbums);
    }
}
