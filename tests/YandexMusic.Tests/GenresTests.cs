using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the genre model, including localized titles and nested sub-genres.</summary>
public sealed class GenresTests
{
    [Fact]
    public void Deserializes_GenresWithTitlesAndSubGenres()
    {
        const string json =
            """
            { "result": [ {
              "id": "rock", "weight": 100, "composerTop": false, "title": "Rock", "fullTitle": "Rock music",
              "titles": { "ru": { "title": "Рок", "fullTitle": "Рок-музыка" }, "en": { "title": "Rock" } },
              "subGenres": [ { "id": "punk", "title": "Punk" } ]
            } ] }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<Genre>>>());
        var genre = Assert.Single(response!.Result!);

        Assert.Equal("rock", genre.Id);
        Assert.Equal("Rock", genre.Title);
        Assert.Equal("Рок", genre.Titles!["ru"].Title);
        Assert.Equal("Рок-музыка", genre.Titles["ru"].FullTitle);
        Assert.Equal("punk", genre.SubGenres![0].Id);
    }
}
