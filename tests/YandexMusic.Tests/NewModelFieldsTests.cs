using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Search;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the fields the API added to tracks, albums, artists, playlists and search results, and
/// that fields it stopped sending everywhere (the owner of artist playlists) read as absent.
/// </summary>
public sealed class NewModelFieldsTests
{
    [Fact]
    public void Track_ReadsChartIsrcClipsAndPodcastFields()
    {
        const string json =
            """
            { "id": "1", "title": "T", "isrc": "RUA000000001", "clipIds": [5, 6], "ugcArtistName": "Me",
              "podcastEpisodeType": "full", "pubDate": "2026-01-02T03:04:05+00:00",
              "chart": { "position": 3, "progress": "up", "listeners": 10, "shift": 1 },
              "r128": { "i": -9.5, "tp": 0.1, "importantSecs": 42 },
              "mixFade": { "inStart": 0.5, "inStop": 1, "outStart": 170, "outStop": 180 } }
            """;

        var track = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<Track>())!;

        Assert.Equal("RUA000000001", track.Isrc);
        Assert.Equal([5L, 6L], track.ClipIds);
        Assert.Equal("Me", track.UgcArtistName);
        Assert.Equal("full", track.PodcastEpisodeType);
        Assert.Equal(2026, track.PubDate!.Value.Year);
        Assert.Equal(3, track.Chart!.Position);
        Assert.Equal(42, track.R128!.ImportantSecs);
        Assert.Equal(180, track.MixFade!.OutStop);
    }

    [Fact]
    public void Album_ReadsWaveTrailerAndActionButton()
    {
        const string json =
            """
            { "id": "1", "title": "A", "durationSec": 3600, "childContent": false, "backgroundImageUrl": "bg",
              "trailer": { "available": true },
              "customWave": { "title": "Wave", "animationUrl": "lottie", "squareAgentAnimation": "sq" },
              "actionButton": { "text": "Buy", "url": "https://x", "color": "#fff", "viewBrowser": true } }
            """;

        var album = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<Album>())!;

        Assert.Equal(3600, album.DurationSec);
        Assert.True(album.Trailer!.Available);
        Assert.Null(album.CustomWave!.Position);
        Assert.Equal("sq", album.CustomWave.SquareAgentAnimation);
        Assert.True(album.ActionButton!.ViewBrowser);
    }

    [Fact]
    public void Playlist_WithoutOwner_ReadsAsAbsent()
    {
        const string json =
            """
            { "uid": 1, "kind": 2, "title": "Top", "artistPlaylistType": "TOP",
              "madeForUser": { "isMadeForUser": true, "caseForms": { "dative": "Ivanu" } },
              "derivedColors": { "accent": "#000" } }
            """;

        var playlist = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<Playlist>())!;

        Assert.Null(playlist.Owner);
        Assert.Equal("TOP", playlist.ArtistPlaylistType);
        Assert.Equal("Ivanu", playlist.MadeForUser!.CaseForms!.Dative);
        Assert.Equal("#000", playlist.DerivedColors!.Accent);
    }

    [Fact]
    public void ArtistBriefInfo_ReadsUpcomingAlbumAndLinks()
    {
        const string json =
            """
            { "result": {
              "artist": { "id": "1", "name": "A", "donationInfo": { "title": "Support", "url": "https://d" } },
              "hasTrailer": true, "backgroundVideoUrl": "v",
              "links": [ { "title": "Site", "href": "https://a", "type": "official" } ],
              "upcomingAlbum": { "id": 9, "title": "Soon", "millisecondsUntilRelease": 1000, "presaved": false,
                                 "artists": [ { "id": "1", "name": "A" } ] },
              "clips": [ { "clipId": 3, "title": "Clip" } ] } }
            """;

        var info = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<ArtistBriefInfo>>())!.Result!;

        Assert.Equal("https://d", info.Artist.DonationInfo!.Url);
        Assert.True(info.HasTrailer);
        Assert.Equal("Site", Assert.Single(info.Links).Title);
        Assert.Equal("Soon", info.UpcomingAlbum!.Title);
        Assert.Single(info.UpcomingAlbum.Artists);
        Assert.Equal("Clip", Assert.Single(info.Clips).Title);
    }

    [Fact]
    public void Search_ReadsClipsAndBanner()
    {
        const string json =
            """
            { "result": { "text": "q",
              "clips": { "total": 1, "perPage": 10, "order": 0, "results": [ { "clipId": 1, "title": "C" } ] },
              "banner": { "text": "Promo", "textForSearch": "q", "imageUrl": "i" } } }
            """;

        var result = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>())!.Result!;

        Assert.Equal("C", Assert.Single(result.Clips!.Results).Title);
        Assert.Equal("Promo", result.Banner!.Text);
    }
}
