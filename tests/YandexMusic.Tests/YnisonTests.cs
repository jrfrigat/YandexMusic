using System.Text.Json;
using YandexMusic.Exceptions;
using YandexMusic.Serialization;
using YandexMusic.Ynison;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the Ynison subsystem: canonical protobuf-JSON parsing of server frames, the wire shape
/// of built requests, and the client's redirect-to-state handshake over a scripted transport.
/// </summary>
public sealed class YnisonTests
{
    private const string RedirectFrame =
        """{"host":"ynison-fallback.music.yandex.ru","redirectTicket":"ticket-1","sessionId":"42","keepAliveParams":{"keepAliveTimeSeconds":25,"keepAliveTimeoutSeconds":30}}""";

    private const string StateFrame =
        """
        {
          "playerState": {
            "status": {"progressMs": "42000", "durationMs": "180000", "paused": false, "playbackSpeed": 1,
                       "version": {"deviceId": "device-a", "version": "123456789012345678", "timestampMs": "1700000000000"}},
            "playerQueue": {
              "entityId": "playlist:1000:123", "entityType": "PLAYLIST", "currentPlayableIndex": 1,
              "playableList": [
                {"playableId": "100", "playableType": "TRACK", "from": "mysmart", "title": "First"},
                {"playableId": "200", "albumIdOptional": "300", "playableType": "TRACK", "from": "mysmart", "title": "Second",
                 "trackInfo": {"trackSourceKey": 3}},
                {"playableId": "400", "playableType": "TRACK", "from": "mysmart", "title": "Third"}
              ],
              "options": {"repeatMode": "NONE"},
              "version": {"deviceId": "device-a", "version": "99", "timestampMs": "5"},
              "entityContext": "USER_TRACKS"}
          },
          "devices": [
            {"info": {"deviceId": "device-a", "title": "Web", "type": "WEB", "appName": "Web Player"},
             "capabilities": {"canBePlayer": true, "canBeRemoteController": false, "volumeGranularity": 100},
             "session": {"id": "777"}, "volumeInfo": {"volume": 0.6}}
          ],
          "activeDeviceIdOptional": "device-a", "timestampMs": "1700000001000", "rid": "abc"
        }
        """;

    [Fact]
    public void ResponseFrame_ParsesCanonicalProtobufJson()
    {
        var state = JsonSerializer.Deserialize(StateFrame, YandexMusicJson.TypeInfo<PutYnisonStateResponse>());

        Assert.NotNull(state);
        Assert.Equal(1_700_000_001_000, state.TimestampMs);
        Assert.Equal("abc", state.Rid);
        Assert.Equal("device-a", state.ActiveDeviceIdOptional);

        var status = state.PlayerState!.Status!;
        Assert.Equal(42_000, status.ProgressMs);
        Assert.Equal(180_000, status.DurationMs);
        Assert.False(status.Paused);
        Assert.Equal(123_456_789_012_345_678, status.Version!.Version);

        var queue = state.PlayerState.PlayerQueue!;
        Assert.Equal(QueueEntityType.Playlist, queue.EntityType);
        Assert.Equal(1, queue.CurrentPlayableIndex);
        Assert.Equal(3, queue.PlayableList.Count);
        Assert.Null(queue.PlayableList[0].AlbumIdOptional);
        Assert.Equal("300", queue.PlayableList[1].AlbumIdOptional);
        Assert.Equal(3, queue.PlayableList[1].TrackInfo!.TrackSourceKey);
        Assert.Equal(RepeatMode.None, queue.Options!.RepeatMode);
        Assert.Equal(QueueEntityContext.UserTracks, queue.EntityContext);

        var device = Assert.Single(state.Devices);
        Assert.Equal(DeviceType.Web, device.Info!.Type);
        Assert.True(device.Capabilities!.CanBePlayer);
        Assert.Equal(777, device.Session!.Id);
        Assert.Equal(0.6, device.VolumeInfo!.Volume, precision: 5);
    }

    [Fact]
    public void ResponseFrame_AcceptsNumericEnumsAndInt64s()
    {
        const string frame =
            """{"playerState":{"playerQueue":{"entityType":5,"currentPlayableIndex":0}},"timestampMs":123,"devices":[{"session":{"id":5}}]}""";

        var state = JsonSerializer.Deserialize(frame, YandexMusicJson.TypeInfo<PutYnisonStateResponse>());

        Assert.NotNull(state);
        Assert.Equal(QueueEntityType.Various, state.PlayerState!.PlayerQueue!.EntityType);
        Assert.Equal(123, state.TimestampMs);
        Assert.Equal(5, state.Devices[0].Session!.Id);
    }

    [Fact]
    public void ResponseFrame_UnknownEnumValue_ReadsAsDefault()
    {
        const string frame = """{"playerState":{"playerQueue":{"entityType":"SOME_FUTURE_TYPE"}}}""";

        var state = JsonSerializer.Deserialize(frame, YandexMusicJson.TypeInfo<PutYnisonStateResponse>());

        Assert.NotNull(state);
        Assert.Equal(QueueEntityType.Unspecified, state.PlayerState!.PlayerQueue!.EntityType);
    }

    [Fact]
    public void RedirectFrame_Parses()
    {
        var redirect = JsonSerializer.Deserialize(RedirectFrame, YandexMusicJson.TypeInfo<RedirectResponse>());

        Assert.NotNull(redirect);
        Assert.Equal("ynison-fallback.music.yandex.ru", redirect.Host);
        Assert.Equal("ticket-1", redirect.RedirectTicket);
        Assert.Equal(42, redirect.SessionId);
        Assert.Equal(25, redirect.KeepAliveParams!.KeepAliveTimeSeconds);
    }

    [Fact]
    public void UpdateFullStateRequest_WritesCanonicalEnumsAndNestedDevice()
    {
        var request = YnisonRequests.CreateUpdateFullStateRequest("device-x", "My Remote");

        var json = JsonSerializer.Serialize(request, YandexMusicJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("DO_NOT_INTERCEPT_BY_DEFAULT", root.GetProperty("activityInterceptionType").GetString());
        var fullState = root.GetProperty("updateFullState");
        Assert.False(fullState.GetProperty("isCurrentlyActive").GetBoolean());
        var device = fullState.GetProperty("device");
        Assert.Equal("device-x", device.GetProperty("info").GetProperty("deviceId").GetString());
        Assert.Equal("My Remote", device.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("WEB", device.GetProperty("info").GetProperty("type").GetString());
        Assert.True(device.GetProperty("capabilities").GetProperty("canBeRemoteController").GetBoolean());
        var queue = fullState.GetProperty("playerState").GetProperty("playerQueue");
        Assert.Equal("VARIOUS", queue.GetProperty("entityType").GetString());
        Assert.Equal("NONE", queue.GetProperty("options").GetProperty("repeatMode").GetString());
        Assert.Equal(-1, queue.GetProperty("currentPlayableIndex").GetInt32());

        // Only the oneof member set by the builder may appear.
        Assert.False(root.TryGetProperty("updatePlayerState", out _));
    }

    [Fact]
    public void SetPausedRequest_ClonesStatusAndFlipsFlag()
    {
        var status = new PlayingStatus(42_000, 180_000, Paused: false, PlaybackSpeed: 1.5)
        {
            Version = new UpdateVersion("device-a", 1, 2),
        };

        var request = YnisonRequests.CreateSetPausedRequest("device-x", status, paused: true);

        var json = JsonSerializer.Serialize(request, YandexMusicJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var newStatus = document.RootElement.GetProperty("updatePlayingStatus").GetProperty("playingStatus");

        Assert.True(newStatus.GetProperty("paused").GetBoolean());
        Assert.Equal(42_000, newStatus.GetProperty("progressMs").GetInt64());
        Assert.Equal(1.5, newStatus.GetProperty("playbackSpeed").GetDouble(), precision: 5);
        Assert.Equal("device-x", newStatus.GetProperty("version").GetProperty("deviceId").GetString());
        Assert.NotEqual(1, newStatus.GetProperty("version").GetProperty("version").GetInt64());
    }

    [Fact]
    public void SetVolumeRequest_ClampsOutOfRangeValues()
    {
        var request = YnisonRequests.CreateSetVolumeRequest("device-x", "device-a", 1.5);

        var json = JsonSerializer.Serialize(request, YandexMusicJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var volumeInfo = document.RootElement.GetProperty("updateVolumeInfo");

        Assert.Equal("device-a", volumeInfo.GetProperty("deviceId").GetString());
        Assert.Equal(1.0, volumeInfo.GetProperty("volumeInfo").GetProperty("volume").GetDouble(), precision: 5);
    }

    [Fact]
    public async Task Client_HandshakesReceivesStateAndSendsCommands()
    {
        var redirectSocket = new FakeSocket([RedirectFrame]);
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([redirectSocket, stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);

        var frames = 0;
        client.StateReceived += (_, _) => frames++;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        var state = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await client.SetPausedAsync(paused: true);
        await client.NextTrackAsync();

        Assert.Equal(123_456_789_012_345_678, client.LatestState!.PlayerState!.Status!.Version!.Version);
        Assert.Equal(42_000, client.LatestState!.PlayerState!.Status!.ProgressMs);
        Assert.True(frames >= 1);

        // The redirect socket saw no ticket; the state socket carried it plus the session id.
        Assert.Equal(["Bearer", "v2"], redirectSocket.Subprotocols!.Take(2));
        Assert.DoesNotContain("Ynison-Redirect-Ticket", Uri.UnescapeDataString(redirectSocket.Subprotocols![2]));
        var stateSubprotocol = Uri.UnescapeDataString(stateSocket.Subprotocols![2]);
        Assert.Contains("Ynison-Device-Id", stateSubprotocol);
        Assert.Contains("Ynison-Redirect-Ticket\":\"ticket-1", stateSubprotocol);
        Assert.Contains("Ynison-Session-Id\":\"42", stateSubprotocol);

        Assert.Equal("wss://ynison.music.yandex.ru/redirector.YnisonRedirectService/GetRedirectToYnison", redirectSocket.Uri!.ToString());
        Assert.Equal("wss://ynison-fallback.music.yandex.ru/ynison_state.YnisonStateService/PutYnisonState", stateSocket.Uri!.ToString());

        var expectedHeaders = new Dictionary<string, string>
        {
            ["Origin"] = "https://music.yandex.ru",
            ["Authorization"] = "OAuth token-1",
        };
        Assert.Equal(expectedHeaders, redirectSocket.Headers);
        Assert.Equal(expectedHeaders, stateSocket.Headers);
        Assert.Equal(TimeSpan.FromSeconds(25), stateSocket.KeepAliveInterval);

        Assert.Equal(3, stateSocket.Sent.Count);
        using var registration = JsonDocument.Parse(stateSocket.Sent[0]);
        Assert.True(registration.RootElement.GetProperty("updateFullState").GetProperty("device")
            .GetProperty("capabilities").GetProperty("canBeRemoteController").GetBoolean());
        using var pause = JsonDocument.Parse(stateSocket.Sent[1]);
        Assert.True(pause.RootElement.GetProperty("updatePlayingStatus").GetProperty("playingStatus").GetProperty("paused").GetBoolean());
        using var next = JsonDocument.Parse(stateSocket.Sent[2]);
        Assert.Equal(2, next.RootElement.GetProperty("updatePlayerState").GetProperty("playerState")
            .GetProperty("playerQueue").GetProperty("currentPlayableIndex").GetInt32());
        Assert.Empty(redirectSocket.Sent);

        await client.DisposeAsync().AsTask().ContinueWith(_ => { }, TaskScheduler.Default);
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Client_WithoutState_RefusesCommands()
    {
        var factory = new FakeSocketFactory([new FakeSocket([RedirectFrame]), new FakeSocket([])]);
        var client = new YnisonClient("token-1", "device-x", null, factory);

        await using (client)
        {
            await Assert.ThrowsAsync<YandexMusicYnisonException>(() => client.SetPausedAsync(paused: true));
        }
    }

    [Fact]
    public async Task WaitForState_TimesOutWhenNoFrameArrives()
    {
        var factory = new FakeSocketFactory([new FakeSocket([RedirectFrame]), new FakeSocket([])]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        await Assert.ThrowsAsync<YandexMusicYnisonException>(
            () => client.WaitForStateAsync(TimeSpan.FromMilliseconds(200)));

        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    private sealed class FakeSocketFactory(IReadOnlyList<FakeSocket> sockets) : IYnisonSocketFactory
    {
        private int _created;

        public IYnisonSocket Create()
        {
            Assert.InRange(_created, 0, sockets.Count - 1);
            return sockets[_created++];
        }
    }

    private sealed class FakeSocket(IEnumerable<string?> incoming) : IYnisonSocket
    {
        private readonly Queue<string?> _incoming = new(incoming);
        private readonly TaskCompletionSource _parked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _disposed;

        public Uri? Uri { get; private set; }

        public List<string>? Subprotocols { get; private set; }

        public Dictionary<string, string>? Headers { get; private set; }

        public TimeSpan KeepAliveInterval { get; private set; }

        public List<string> Sent { get; } = [];

        public Task ConnectAsync(
            Uri uri,
            IReadOnlyList<string> subprotocols,
            IReadOnlyDictionary<string, string> headers,
            TimeSpan keepAliveInterval,
            CancellationToken cancellationToken)
        {
            Uri = uri;
            Subprotocols = [.. subprotocols];
            Headers = new Dictionary<string, string>(headers);
            KeepAliveInterval = keepAliveInterval;
            return Task.CompletedTask;
        }

        public async Task<string?> ReceiveTextAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_incoming.TryDequeue(out var frame))
                {
                    return frame;
                }

                await _parked.Task.WaitAsync(cancellationToken);
            }
        }

        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return ValueTask.CompletedTask;
            }

            // Unblock a parked receive; the cancellation surfaces to the client's stop path.
            _parked.TrySetCanceled();
            return ValueTask.CompletedTask;
        }
    }
}
