using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Playlists;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the playlist model, including the owner, track wrappers and visibility.</summary>
public sealed class PlaylistDeserializationTests
{
    private const string PlaylistEnvelope =
        """
        {
          "result": {
            "owner": { "uid": 123, "login": "user", "name": "User", "sex": "unknown", "verified": false },
            "playlistUuid": "uuid-1", "uid": 123, "kind": 1000, "title": "My Rock",
            "description": "best rock", "revision": 5, "snapshot": 3, "trackCount": 2,
            "visibility": "public", "collective": false, "available": true,
            "created": "2020-01-01T00:00:00+03:00", "modified": "2021-06-01T12:00:00+03:00",
            "durationMs": 600000, "likesCount": 10,
            "cover": { "type": "pic", "uri": "u" },
            "tracks": [
              { "id": 4, "originalIndex": 0, "timestamp": "2020-01-01T00:00:00+03:00",
                "track": { "id": "4", "title": "Just In Time", "type": "music", "durationMs": 315060 } }
            ],
            "pager": { "total": 2, "page": 0, "perPage": 100 }
          }
        }
        """;

    [Fact]
    public void Deserializes_Playlist()
    {
        var response = JsonSerializer.Deserialize(PlaylistEnvelope, YandexMusicJson.TypeInfo<ApiResponse<Playlist>>());
        var playlist = response!.Result!;

        Assert.Equal("My Rock", playlist.Title);
        Assert.Equal(1000, playlist.Kind);
        Assert.Equal(PlaylistVisibility.Public, playlist.Visibility);
        Assert.Equal("user", playlist.Owner.Login);
        Assert.Equal(2020, playlist.Created!.Value.Year);
        Assert.Equal(2, playlist.Pager!.Total);
    }

    [Fact]
    public void Deserializes_PlaylistTracks_WithEmbeddedTrack()
    {
        var response = JsonSerializer.Deserialize(PlaylistEnvelope, YandexMusicJson.TypeInfo<ApiResponse<Playlist>>());
        var item = Assert.Single(response!.Result!.Tracks);

        Assert.Equal("4", item.Id);
        Assert.Equal(0, item.OriginalIndex);
        Assert.NotNull(item.Track);
        Assert.Equal("Just In Time", item.Track!.Title);
    }

    [Theory]
    [InlineData("\"id\": 42", "42")]
    [InlineData("\"id\": \"42\"", "42")]
    public void TrackShortId_ReadsNumberOrString(string idJson, string expected)
    {
        var json = $$"""{ "result": { "title": "p", "tracks": [ { {{idJson}}, "originalIndex": 0 } ] } }""";
        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Playlist>>());

        Assert.Equal(expected, response!.Result!.Tracks[0].Id);
    }
}
