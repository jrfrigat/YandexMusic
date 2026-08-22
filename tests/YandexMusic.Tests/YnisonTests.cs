using System.Net.WebSockets;
using System.Text.Json;
using YandexMusic.Exceptions;
using YandexMusic.Ynison;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the Ynison subsystem: snake_case protobuf-JSON parsing of server frames, the wire shape
/// of built requests, and the client's redirect-to-state handshake over a scripted transport.
/// </summary>
public sealed class YnisonTests
{
    private const string RedirectFrame =
        """{"host":"ynison-fallback.music.yandex.ru","redirect_ticket":"ticket-1","session_id":"42","keep_alive_params":{"keep_alive_time_seconds":25,"keep_alive_timeout_seconds":30}}""";

    private const string StateFrame =
        """
        {
          "player_state": {
            "status": {"progress_ms": "42000", "duration_ms": "180000", "paused": false, "playback_speed": 1,
                       "version": {"device_id": "device-a", "version": "123456789012345678", "timestamp_ms": "1700000000000"}},
            "player_queue": {
              "entity_id": "playlist:1000:123", "entity_type": "PLAYLIST", "current_playable_index": 1,
              "playable_list": [
                {"playable_id": "100", "playable_type": "TRACK", "from": "mysmart", "title": "First"},
                {"playable_id": "200", "album_id_optional": "300", "playable_type": "TRACK", "from": "mysmart", "title": "Second",
                 "track_info": {"track_source_key": 3}},
                {"playable_id": "400", "playable_type": "TRACK", "from": "mysmart", "title": "Third"}
              ],
              "options": {"repeat_mode": "NONE"},
              "version": {"device_id": "device-a", "version": "99", "timestamp_ms": "5"},
              "entity_context": "USER_TRACKS"}
          },
          "devices": [
            {"info": {"device_id": "device-a", "title": "Web", "type": "WEB", "app_name": "Web Player"},
             "capabilities": {"can_be_player": true, "can_be_remote_controller": false, "volume_granularity": 100},
             "session": {"id": "777"}, "volume_info": {"volume": 0.6}}
          ],
          "active_device_id_optional": "device-a", "timestamp_ms": "1700000001000", "rid": "abc"
        }
        """;

    [Fact]
    public void ResponseFrame_ParsesSnakeCaseProtobufJson()
    {
        var state = JsonSerializer.Deserialize(StateFrame, YnisonJson.TypeInfo<PutYnisonStateResponse>());

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
            """{"player_state":{"player_queue":{"entity_type":5,"current_playable_index":0}},"timestamp_ms":123,"devices":[{"session":{"id":5}}]}""";

        var state = JsonSerializer.Deserialize(frame, YnisonJson.TypeInfo<PutYnisonStateResponse>());

        Assert.NotNull(state);
        Assert.Equal(QueueEntityType.Various, state.PlayerState!.PlayerQueue!.EntityType);
        Assert.Equal(123, state.TimestampMs);
        Assert.Equal(5, state.Devices[0].Session!.Id);
    }

    [Fact]
    public void ResponseFrame_UnknownEnumValue_ReadsAsDefault()
    {
        const string frame = """{"player_state":{"player_queue":{"entity_type":"SOME_FUTURE_TYPE"}}}""";

        var state = JsonSerializer.Deserialize(frame, YnisonJson.TypeInfo<PutYnisonStateResponse>());

        Assert.NotNull(state);
        Assert.Equal(QueueEntityType.Unspecified, state.PlayerState!.PlayerQueue!.EntityType);
    }

    [Fact]
    public void RedirectFrame_Parses()
    {
        var redirect = JsonSerializer.Deserialize(RedirectFrame, YnisonJson.TypeInfo<RedirectResponse>());

        Assert.NotNull(redirect);
        Assert.Equal("ynison-fallback.music.yandex.ru", redirect.Host);
        Assert.Equal("ticket-1", redirect.RedirectTicket);
        Assert.Equal(42, redirect.SessionId);
        Assert.Equal(25, redirect.KeepAliveParams!.KeepAliveTimeSeconds);
    }

    [Fact]
    public void UpdateFullStateRequest_WritesSnakeCaseEnumsAndNestedDevice()
    {
        var request = YnisonRequests.CreateUpdateFullStateRequest("device-x", "My Remote");

        var json = JsonSerializer.Serialize(request, YnisonJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("DO_NOT_INTERCEPT_BY_DEFAULT", root.GetProperty("activity_interception_type").GetString());
        var fullState = root.GetProperty("update_full_state");
        Assert.False(fullState.GetProperty("is_currently_active").GetBoolean());
        var device = fullState.GetProperty("device");
        Assert.Equal("device-x", device.GetProperty("info").GetProperty("device_id").GetString());
        Assert.Equal("My Remote", device.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("WEB", device.GetProperty("info").GetProperty("type").GetString());
        Assert.True(device.GetProperty("capabilities").GetProperty("can_be_remote_controller").GetBoolean());
        var queue = fullState.GetProperty("player_state").GetProperty("player_queue");
        Assert.Equal("VARIOUS", queue.GetProperty("entity_type").GetString());
        Assert.Equal("NONE", queue.GetProperty("options").GetProperty("repeat_mode").GetString());
        Assert.Equal(-1, queue.GetProperty("current_playable_index").GetInt32());

        // Only the oneof member set by the builder may appear.
        Assert.False(root.TryGetProperty("update_player_state", out _));
    }

    [Fact]
    public void SetPausedRequest_ClonesStatusAndFlipsFlag()
    {
        var status = new PlayingStatus(42_000, 180_000, Paused: false, PlaybackSpeed: 1.5)
        {
            Version = new UpdateVersion("device-a", 1, 2),
        };

        var request = YnisonRequests.CreateSetPausedRequest("device-x", status, paused: true);

        var json = JsonSerializer.Serialize(request, YnisonJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var newStatus = document.RootElement.GetProperty("update_playing_status").GetProperty("playing_status");

        Assert.True(newStatus.GetProperty("paused").GetBoolean());
        Assert.Equal(42_000, newStatus.GetProperty("progress_ms").GetInt64());
        Assert.Equal(1.5, newStatus.GetProperty("playback_speed").GetDouble(), precision: 5);
        Assert.Equal("device-x", newStatus.GetProperty("version").GetProperty("device_id").GetString());
        Assert.NotEqual(1, newStatus.GetProperty("version").GetProperty("version").GetInt64());
    }

    [Fact]
    public void SetVolumeRequest_ClampsOutOfRangeValues()
    {
        var request = YnisonRequests.CreateSetVolumeRequest("device-x", "device-a", 1.5);

        var json = JsonSerializer.Serialize(request, YnisonJson.TypeInfo<PutYnisonStateRequest>());
        using var document = JsonDocument.Parse(json);
        var volumeInfo = document.RootElement.GetProperty("update_volume_info");

        Assert.Equal("device-a", volumeInfo.GetProperty("device_id").GetString());
        Assert.Equal(1.0, volumeInfo.GetProperty("volume_info").GetProperty("volume").GetDouble(), precision: 5);
    }

    [Fact]
    public async Task Client_HandshakesReceivesStateAndSendsCommands()
    {
        var redirectSocket = new FakeSocket([RedirectFrame]);
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([redirectSocket, stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        var frames = 0;
        client.StateReceived += (_, _) => frames++;

        await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await client.SetPausedAsync(paused: true);
        await client.NextTrackAsync();

        Assert.Equal(123_456_789_012_345_678, client.LatestState!.PlayerState!.Status!.Version!.Version);
        Assert.Equal(42_000, client.LatestState.PlayerState!.Status!.ProgressMs);
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
        Assert.True(registration.RootElement.GetProperty("update_full_state").GetProperty("device")
            .GetProperty("capabilities").GetProperty("can_be_remote_controller").GetBoolean());
        using var pause = JsonDocument.Parse(stateSocket.Sent[1]);
        Assert.True(pause.RootElement.GetProperty("update_playing_status").GetProperty("playing_status").GetProperty("paused").GetBoolean());
        using var next = JsonDocument.Parse(stateSocket.Sent[2]);
        Assert.Equal(2, next.RootElement.GetProperty("update_player_state").GetProperty("player_state")
            .GetProperty("player_queue").GetProperty("current_playable_index").GetInt32());
        Assert.Empty(redirectSocket.Sent);

        await client.DisposeAsync().AsTask().ContinueWith(_ => { }, TaskScheduler.Default);
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task PlayOnDevice_SwitchesActiveDeviceAndResumes()
    {
        var redirectSocket = new FakeSocket([RedirectFrame]);
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([redirectSocket, stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        _ = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await client.PlayOnDeviceAsync("device-a");

        // The session is already playing, so only the registration and the device switch go out: a
        // redundant resume makes the device that was playing flap its own pause flag.
        Assert.Equal(2, stateSocket.Sent.Count);
        using var activation = JsonDocument.Parse(stateSocket.Sent[1]);
        var activeDevice = activation.RootElement.GetProperty("update_active_device");
        Assert.Equal("device-a", activeDevice.GetProperty("device_id_optional").GetString());

        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task PlayOnDevice_ResumesWhenTheSessionIsPaused()
    {
        var pausedFrame = StateFrame.Replace("\"paused\": false", "\"paused\": true", StringComparison.Ordinal);
        var stateSocket = new FakeSocket([pausedFrame]);
        var factory = new FakeSocketFactory([new FakeSocket([RedirectFrame]), stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        _ = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await client.PlayOnDeviceAsync("device-a");

        // Registration, the device switch, and this time a resume, because it was actually paused.
        Assert.Equal(3, stateSocket.Sent.Count);
        using var resume = JsonDocument.Parse(stateSocket.Sent[2]);
        var status = resume.RootElement.GetProperty("update_playing_status").GetProperty("playing_status");
        Assert.False(status.GetProperty("paused").GetBoolean());

        // A paused track has not moved, so its position is sent back untouched.
        Assert.Equal(42_000, status.GetProperty("progress_ms").GetInt64());

        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task PauseCommand_CarriesThePositionBroughtUpToNow()
    {
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([new FakeSocket([RedirectFrame]), stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        _ = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await Task.Delay(250, cts.Token);
        await client.SetPausedAsync(paused: true);

        // The frame said 42000 ms and Ynison never sends progress ticks, so replaying that number a
        // quarter-second later would publish a rewind to the whole session.
        using var command = JsonDocument.Parse(stateSocket.Sent[^1]);
        var progress = command.RootElement
            .GetProperty("update_playing_status").GetProperty("playing_status")
            .GetProperty("progress_ms").GetInt64();
        Assert.InRange(progress, 42_200, 43_500);

        await client.DisposeAsync();
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

    [Fact]
    public async Task WaitForState_ReportsWhyTheHandshakeFailed_InsteadOfWaitingOutTheTimeout()
    {
        // The redirector hangs up instead of answering: a fatal, non-transient failure.
        var factory = new FakeSocketFactory([new FakeSocket([null])]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        var run = Task.Run(() => client.RunAsync(CancellationToken.None));

        // The timeout is deliberately long: the wait has to end on the failure, not on the clock.
        var failure = await Assert
            .ThrowsAsync<YandexMusicYnisonException>(() => client.WaitForStateAsync(TimeSpan.FromMinutes(5)))
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Contains("redirector", failure.Message, StringComparison.OrdinalIgnoreCase);
        await Assert.ThrowsAsync<YandexMusicYnisonException>(() => run).WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Command_OnADroppedSocket_FailsAsAYnisonException()
    {
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([new FakeSocket([RedirectFrame]), stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        _ = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        stateSocket.SendFailure = new WebSocketException(WebSocketError.ConnectionClosedPrematurely);

        // Callers handle one exception type; the raw transport failure stays as the cause.
        var failure = await Assert.ThrowsAsync<YandexMusicYnisonException>(() => client.SetPausedAsync(paused: true));
        _ = Assert.IsType<WebSocketException>(failure.InnerException);

        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Stopping_ClosesBothSocketsGracefully()
    {
        var redirectSocket = new FakeSocket([RedirectFrame]);
        var stateSocket = new FakeSocket([StateFrame]);
        var factory = new FakeSocketFactory([redirectSocket, stateSocket]);
        await using var client = new YnisonClient("token-1", "device-x", null, factory);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var run = Task.Run(() => client.RunAsync(cts.Token));

        _ = await client.WaitForStateAsync(TimeSpan.FromSeconds(10));
        await client.DisposeAsync();
        await run.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(redirectSocket.Closes > 0, "the redirector socket was aborted without a close handshake");
        Assert.True(stateSocket.Closes > 0, "the state socket was aborted without a close handshake");
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

        /// <summary>Set to make the next send fail, standing in for a socket that dropped.</summary>
        public Exception? SendFailure { get; set; }

        /// <summary>How many times a close handshake was requested.</summary>
        public int Closes { get; private set; }

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
            if (SendFailure is not null)
            {
                throw SendFailure;
            }

            Sent.Add(message);
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Closes++;
            return Task.CompletedTask;
        }

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
