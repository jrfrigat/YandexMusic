using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Account;
using YandexMusic.Models.Library;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the account and library models deserialize their envelopes.</summary>
public sealed class AccountLibraryTests
{
    [Fact]
    public void Deserializes_AccountStatus()
    {
        const string json =
            """
            { "result": {
              "account": { "uid": 123, "login": "user", "fullName": "John Doe", "region": 225,
                "now": "2026-06-29T12:00:00+03:00", "serviceAvailable": true },
              "permissions": { "until": "2027-01-01T00:00:00+03:00", "values": [ "feature-a" ], "default": [ "feature-b" ] }
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<AccountStatus>>());
        var status = response!.Result!;

        Assert.Equal(123, status.Account.Uid);
        Assert.Equal("user", status.Account.Login);
        Assert.True(status.Account.ServiceAvailable);
        Assert.Equal("feature-a", status.Permissions!.Values[0]);
    }

    [Fact]
    public void Deserializes_LikedTracks()
    {
        const string json =
            """
            { "result": { "library": { "uid": 123, "revision": 5, "tracks": [
              { "id": 4, "albumId": "3", "timestamp": "2020-01-01T00:00:00+03:00" }
            ] } } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<LikedTracksEnvelope>>());
        var library = response!.Result!.Library;

        Assert.Equal(123, library.Uid);
        Assert.Equal(5, library.Revision);
        Assert.Equal("4", library.Tracks[0].Id);
        Assert.Equal("3", library.Tracks[0].AlbumId);
    }

    [Fact]
    public void Deserializes_LikedAlbums()
    {
        const string json =
            """{ "result": [ { "id": 3, "timestamp": "2020-01-01T00:00:00+03:00", "album": { "id": 3, "title": "Taller Children", "metaType": "music" } } ] }""";

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<LikedAlbum>>>());
        var liked = Assert.Single(response!.Result!);

        Assert.Equal(3, liked.Id);
        Assert.Equal("Taller Children", liked.Album!.Title);
    }
}
