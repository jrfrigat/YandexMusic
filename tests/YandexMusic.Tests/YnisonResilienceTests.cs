using System.Text.Json;
using YandexMusic.Exceptions;
using YandexMusic.Tests.Infrastructure;
using YandexMusic.Ynison;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies how the Ynison client handles the server's error frames (reconnecting on recoverable
/// ones, stopping on terminal ones) and that track switching follows playback order.
/// </summary>
public sealed class YnisonResilienceTests
{
    private const string RedirectFrame =
        """{"host":"ynison-fallback.music.yandex.ru","redirect_ticket":"ticket-1","session_id":"42"}""";

    private const string StateFrame =
        """
        {"player_state": {
           "status": {"progress_ms": "1000", "duration_ms": "180000", "paused": false, "playback_speed": 1},
           "player_queue": {"entity_id": "e", "entity_type": "PLAYLIST", "current_playable_index": 0,
             "playable_list": [{"playable_id": "1", "playable_type": "TRACK", "from": "f", "title": "A"}]}},
         "devices": [], "active_device_id_optional": "", "timestamp_ms": "1", "rid": "r"}
        """;

    [Fact]
    public void ErrorFrame_ParsesCodesBackoffAndKind()
    {
        const string frame =
            """
            {"error": {"message": "Bad request", "grpc_code": 3, "http_code": "400",
              "details": {"ynison-error-code": "123", "ynison-backoff-millis": "0:1000:5000",
                          "ynison-go-away-for-seconds": "0"}}}
            """;

        Assert.True(YnisonServerErrors.TryParse(frame, out var error));
        Assert.Equal("Bad request", error!.Message);
        Assert.Equal(3, error.GrpcCode);
        Assert.Equal(400, error.HttpCode);
        Assert.Equal("123", error.ErrorCode);
        Assert.Equal([TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)], error.Backoff);
        Assert.Null(error.GoAway);
        Assert.Equal(YnisonServerErrorKind.Other, error.Kind);
        Assert.False(error.IsTerminal);
    }

    [Theory]
    [InlineData("""{"error": {"message": "m", "grpc_code": 16}}""", YnisonServerErrorKind.Unauthorized)]
    [InlineData("""{"error": {"message": "m", "http_code": 403}}""", YnisonServerErrorKind.Unauthorized)]
    [InlineData("""{"error": {"message": "m", "extra_headers": {"ynison-error-code": "400090001"}}}""", YnisonServerErrorKind.DeviceDisplaced)]
    public void ErrorFrame_ClassifiesTerminalErrors(string frame, YnisonServerErrorKind kind)
    {
        Assert.True(YnisonServerErrors.TryParse(frame, out var error));
        Assert.Equal(kind, error!.Kind);
        Assert.True(error.IsTerminal);
    }

    [Fact]
    public void ErrorFrame_GoAwayIsTerminal()
    {
        Assert.True(YnisonServerErrors.TryParse(
            """{"error": {"details": {"ynison-go-away-for-seconds": "60"}}}""", out var error));
        Assert.Equal(TimeSpan.FromMinutes(1), error!.GoAway);
        Assert.True(error.IsTerminal);
        Assert.Equal("Unknown Ynison error.", error.Message);
    }

    [Fact]
    public void StateFrame_IsNotMistakenForAnError()
    {
        Assert.False(YnisonServerErrors.TryParse(StateFrame, out _));
        Assert.False(YnisonServerErrors.TryParse("""{"player_state": {"error": "not top level"}}""", out _));
    }

    [Fact]
    public async Task RecoverableErrorFrame_ReconnectsAndReportsTheError()
    {
        const string error = """{"error": {"message": "try again", "details": {"ynison-backoff-millis": "0"}}}""";
        var factory = new FakeSocketFactory(
        [
            new FakeSocket([RedirectFrame]), new FakeSocket([error]),
            new FakeSocket([RedirectFrame]), new FakeSocket([StateFrame]),
        ]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        var reported = new List<Exception>();
        client.ConnectionError += (_, ex) => reported.Add(ex);
        var run = Task.Run(() => client.RunAsync(CancellationToken.None));

        var state = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));

        Assert.Equal("1", state.PlayerState!.PlayerQueue!.PlayableList[0].PlayableId);
        var reportedError = Assert.IsType<YandexMusicYnisonServerException>(Assert.Single(reported));
        Assert.Equal("try again", reportedError.Message);
        Assert.Equal(4, factory.Created);

        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task TerminalErrorFrame_EndsTheRunWithoutReconnecting()
    {
        var factory = new FakeSocketFactory(
        [
            new FakeSocket([RedirectFrame]),
            new FakeSocket(["""{"error": {"message": "bad token", "grpc_code": 16}}"""]),
        ]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);

        var failure = await Assert
            .ThrowsAsync<YandexMusicYnisonServerException>(() => client.RunAsync(CancellationToken.None))
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(YnisonServerErrorKind.Unauthorized, failure.Kind);
        Assert.Equal(2, factory.Created);
        Assert.Null(client.LatestState);
    }

    [Fact]
    public async Task ErrorFrameFromTheRedirector_IsReportedAsIs()
    {
        var factory = new FakeSocketFactory(
        [
            new FakeSocket(["""{"error": {"message": "displaced", "details": {"ynison-error-code": "400090001"}}}"""]),
        ]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);

        var failure = await Assert
            .ThrowsAsync<YandexMusicYnisonServerException>(() => client.RunAsync(CancellationToken.None))
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(YnisonServerErrorKind.DeviceDisplaced, failure.Kind);
    }

    [Fact]
    public async Task ReconnectLimit_StopsAfterTheGivenNumberOfFailures()
    {
        const string error = """{"error": {"message": "busy", "details": {"ynison-backoff-millis": "0"}}}""";
        var factory = new FakeSocketFactory(
        [
            new FakeSocket([RedirectFrame]), new FakeSocket([error]),
            new FakeSocket([RedirectFrame]), new FakeSocket([error]),
        ]);
        await using var client = new YnisonClient("token-1", "device-x", new YnisonClientOptions { MaxReconnectAttempts = 1 }, factory);

        var failure = await Assert
            .ThrowsAsync<YandexMusicYnisonException>(() => client.RunAsync(CancellationToken.None))
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.IsType<YandexMusicYnisonServerException>(failure.InnerException);
        Assert.Equal(4, factory.Created);
    }

    [Theory]
    [InlineData(1, 0)] // shuffled order 2 -> 0 -> 1, current 2: next is 0, not list index 3.
    [InlineData(-1, null)] // current is first in shuffled order: no previous.
    public void ChangeTrack_FollowsTheShuffledOrder(int delta, int? expected)
    {
        var state = Queue(current: 2, shuffle: "[2, 0, 1]", repeat: "NONE");

        if (expected is null)
        {
            Assert.Throws<YandexMusicYnisonException>(() => YnisonRequests.CreateChangeTrackRequest("me", state, delta));
            return;
        }

        var request = YnisonRequests.CreateChangeTrackRequest("me", state, delta);
        Assert.Equal(expected, request.UpdatePlayerState!.PlayerState!.PlayerQueue!.CurrentPlayableIndex);
    }

    [Fact]
    public void ChangeTrack_AtTheEndWithoutRepeat_ReportsTheBoundary()
    {
        var state = Queue(current: 2, shuffle: null, repeat: "NONE");

        var failure = Assert.Throws<YandexMusicYnisonException>(() => YnisonRequests.CreateNextTrackRequest("me", state));
        Assert.Contains("+1", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ChangeTrack_WithRepeatAll_WrapsAround()
    {
        var next = YnisonRequests.CreateNextTrackRequest("me", Queue(current: 2, shuffle: null, repeat: "ALL"));
        var previous = YnisonRequests.CreatePreviousTrackRequest("me", Queue(current: 0, shuffle: null, repeat: "ALL"));

        Assert.Equal(0, next.UpdatePlayerState!.PlayerState!.PlayerQueue!.CurrentPlayableIndex);
        Assert.Equal(2, previous.UpdatePlayerState!.PlayerState!.PlayerQueue!.CurrentPlayableIndex);
    }

    [Fact]
    public void PlaybackOrder_IgnoresAStaleShuffle()
    {
        var queue = Queue(current: 0, shuffle: "[1, 0]", repeat: "NONE").PlayerQueue!;

        Assert.Equal([0, 1, 2], YnisonQueueNavigation.GetPlaybackOrder(queue));
        Assert.Equal("t0", YnisonQueueNavigation.GetCurrentPlayable(queue)!.PlayableId);
    }

    private static PlayerState Queue(int current, string? shuffle, string repeat)
    {
        var shuffleJson = shuffle is null ? string.Empty : ", \"shuffle_optional\": {\"playable_indices\": " + shuffle + "}";
        var json =
            "{\"player_state\": {" +
            "\"status\": {\"progress_ms\": \"0\", \"duration_ms\": \"0\", \"paused\": false, \"playback_speed\": 1}," +
            "\"player_queue\": {\"entity_id\": \"e\", \"entity_type\": \"PLAYLIST\", \"current_playable_index\": " + current + "," +
            "\"options\": {\"repeat_mode\": \"" + repeat + "\"}," +
            "\"playable_list\": [" +
            "{\"playable_id\": \"t0\", \"playable_type\": \"TRACK\", \"from\": \"f\", \"title\": \"0\"}," +
            "{\"playable_id\": \"t1\", \"playable_type\": \"TRACK\", \"from\": \"f\", \"title\": \"1\"}," +
            "{\"playable_id\": \"t2\", \"playable_type\": \"TRACK\", \"from\": \"f\", \"title\": \"2\"}]" +
            shuffleJson + "}}}";

        return JsonSerializer.Deserialize(json, YnisonJson.TypeInfo<PutYnisonStateResponse>())!.PlayerState!;
    }
}
