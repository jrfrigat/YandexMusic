using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Pins;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Pins models.</summary>
public sealed class PinsTests
{
    [Fact]
    public void Deserializes_PinsList_WithMixedEntries()
    {
        const string json =
            """
            { "result": { "pins": [
              { "type": "artist_item", "data": { "id": 12345, "name": "Artist" } },
              { "type": "playlist_item", "data": { "uid": 67890, "kind": 1003,
                "playlist_uuid": "abc-uuid", "title": "My Mix" } } ] } }
            """;

        var pins = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PinsList>>())!.Result;

        Assert.NotNull(pins);
        Assert.Equal(2, pins!.Pins!.Count);

        var artist = pins.Pins[0];
        Assert.Equal("artist_item", artist.Type);
        Assert.Equal(12345L, artist.Data!.Id);
        Assert.Equal("Artist", artist.Data.Name);

        var playlist = pins.Pins[1].Data!;
        Assert.Equal(67890L, playlist.Uid);
        Assert.Equal(1003L, playlist.Kind);
        Assert.Equal("abc-uuid", playlist.PlaylistUuid);
        Assert.Equal("My Mix", playlist.Title);
    }

    [Fact]
    public void Deserializes_Pin_WithCoverAndRestrictions()
    {
        const string json =
            """
            { "result": { "type": "album_item", "data": { "id": 42, "title": "Album",
              "cover": { "type": "pic", "uri": "avatars/%%" },
              "content_restrictions": { "available": true, "disclaimers": [ "explicit" ] } } } }
            """;

        var pin = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Pin>>())!.Result;

        Assert.NotNull(pin);
        Assert.Equal("album_item", pin!.Type);
        Assert.Equal(42L, pin.Data!.Id);
        Assert.Equal("avatars/%%", pin.Data.Cover!.Uri);
        Assert.True(pin.Data.ContentRestrictions!.Available);
        Assert.Equal("explicit", Assert.Single(pin.Data.ContentRestrictions.Disclaimers!));
    }
}
