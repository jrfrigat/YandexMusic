using YandexMusic.Endpoints;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Queue;
using YandexMusic.Tests.Infrastructure;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the exact wire conventions (URL, HTTP verb, headers and form body) each endpoint builds,
/// using a <see cref="RecordingConnection"/> so no network call is made. These guard the brittle
/// string-built request conventions against regressions.
/// </summary>
public sealed class EndpointRequestTests
{
    [Fact]
    public async Task Tracks_GetMany_PostsIdsAndPositionsInBody()
    {
        var connection = new RecordingConnection();
        await new TracksClient(connection).GetManyAsync(["1", "2", "3"]);

        Assert.Equal(HttpMethod.Post, connection.Method);
        Assert.Equal("/tracks", connection.Url);
        Assert.Contains("track-ids=1%2C2%2C3", connection.Body);
        Assert.Contains("with-positions=True", connection.Body);
    }

    [Fact]
    public async Task Tracks_GetSingle_UsesPathGet()
    {
        var connection = new RecordingConnection();
        await new TracksClient(connection).GetAsync("42");

        Assert.Equal(HttpMethod.Get, connection.Method);
        Assert.Equal("/tracks/42", connection.Url);
    }

    [Fact]
    public async Task Tracks_GetLyrics_AppendsSignAndTimestamp()
    {
        var connection = new RecordingConnection();
        await new TracksClient(connection).GetLyricsAsync("42", "LRC");

        Assert.StartsWith("/tracks/42/lyrics?format=LRC&timeStamp=", connection.Url);
        Assert.Contains("&sign=", connection.Url);
    }

    [Fact]
    public async Task Albums_GetMany_PostsAlbumIds()
    {
        var connection = new RecordingConnection();
        await new AlbumsClient(connection).GetManyAsync(["10", "20"]);

        Assert.Equal(HttpMethod.Post, connection.Method);
        Assert.Equal("/albums", connection.Url);
        Assert.Contains("album-ids=10%2C20", connection.Body);
    }

    [Fact]
    public async Task Library_GetLikedAlbums_SendsRichDefaultTrue()
    {
        var connection = new RecordingConnection();
        await new LibraryClient(connection).GetLikedAlbumsAsync("user");

        Assert.Equal("/users/user/likes/albums?rich=True", connection.Url);
    }

    [Fact]
    public async Task Library_GetDislikedTracks_UsesUnderscoreRevisionKey()
    {
        var connection = new RecordingConnection();
        await new LibraryClient(connection).GetDislikedTracksAsync("user");

        Assert.Contains("/dislikes/tracks?if_modified_since_revision=0", connection.Url);
    }

    [Fact]
    public async Task Library_AddLikedTracks_PostsToAddMultiple()
    {
        var connection = new RecordingConnection();
        await new LibraryClient(connection).AddLikedTracksAsync("user", ["1", "2"]);

        Assert.Equal(HttpMethod.Post, connection.Method);
        Assert.Equal("/users/user/likes/tracks/add-multiple", connection.Url);
        Assert.Contains("track-ids=1%2C2", connection.Body);
    }

    [Fact]
    public async Task Queue_UpdatePosition_SendsDeviceHeaderAndCapitalizedBool()
    {
        var connection = new RecordingConnection();
        await new QueueClient(connection).UpdatePositionAsync("q1", 3);

        Assert.Equal("/queues/q1/update-position?currentIndex=3", connection.Url);
        Assert.NotNull(connection.Headers);
        Assert.True(connection.Headers!.ContainsKey("X-Yandex-Music-Device"));
        Assert.Contains("isInteractive=False", connection.Body);
    }

    [Fact]
    public async Task Queue_UpdatePosition_HonorsExplicitDevice()
    {
        var connection = new RecordingConnection();
        await new QueueClient(connection).UpdatePositionAsync("q1", 0, device: "os=Test; device_id=abc");

        Assert.Equal("os=Test; device_id=abc", connection.Headers!["X-Yandex-Music-Device"]);
    }

    [Fact]
    public async Task Playlists_GetByIds_DoesNotDoubleEncodeCommas()
    {
        var connection = new RecordingConnection();
        await new PlaylistsClient(connection).GetByIdsAsync(["100:1", "100:2"]);

        // Commas separate ids and must stay literal; only each id is escaped (the colon -> %3A).
        Assert.Equal("/playlists?playlistIds=100%3A1,100%3A2", connection.Url);
    }

    [Theory]
    [InlineData(PlaylistVisibility.Public, "visibility=public")]
    [InlineData(PlaylistVisibility.Private, "visibility=private")]
    public async Task Playlists_Create_MapsVisibilityEnumToWire(PlaylistVisibility visibility, string expected)
    {
        var connection = new RecordingConnection();
        await new PlaylistsClient(connection).CreateAsync("user", "My list", visibility);

        Assert.Equal("/users/user/playlists/create", connection.Url);
        Assert.Contains(expected, connection.Body);
    }

    [Fact]
    public async Task Disclaimers_BuildsEntityScopedPath()
    {
        var connection = new RecordingConnection();
        await new DisclaimersClient(connection).GetArtistDisclaimerAsync("79215");

        Assert.Equal("/artists/79215/disclaimer", connection.Url);
    }

    [Fact]
    public async Task Queue_GetQueues_ReturnsEmptyWhenNoResult()
    {
        var connection = new RecordingConnection { NextResult = null };
        var queues = await new QueueClient(connection).GetQueuesAsync();

        Assert.Empty(queues);
        Assert.Equal("/queues", connection.Url);
    }

    [Fact]
    public async Task Queue_GetQueues_UnwrapsQueuesList()
    {
        var connection = new RecordingConnection
        {
            NextResult = new QueuesListResult { Queues = [new QueueItem { Id = "q1" }] },
        };
        var queues = await new QueueClient(connection).GetQueuesAsync();

        Assert.Equal("q1", Assert.Single(queues).Id);
    }
}
