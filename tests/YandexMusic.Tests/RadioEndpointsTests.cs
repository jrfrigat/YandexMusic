using System.Text.Json;
using YandexMusic.Endpoints;
using YandexMusic.Http;
using YandexMusic.Models.Radio;
using YandexMusic.Serialization;
using YandexMusic.Tests.Infrastructure;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies the rotor station, wave and session endpoints: the JSON bodies the API now requires,
/// the timestamp format it accepts, and the session, wave and generative-station models.
/// </summary>
public sealed class RadioEndpointsTests
{
    private static readonly DateTimeOffset At = new(2024, 1, 2, 3, 4, 5, 678, TimeSpan.FromHours(3));

    [Fact]
    public async Task StationFeedback_SendsJsonWithIsoTimestampAndQueryIds()
    {
        var connection = new RecordingConnection { NextResult = "ok" };

        var ok = await new RadioClient(connection).SendStationFeedbackAsync(
            "user:onyourwave", "trackFinished", At, from: "web", batchId: "b 1", totalPlayedSeconds: 12.5, trackId: "10:20", streamId: "s1");

        Assert.True(ok);
        Assert.Equal("/rotor/station/user%3Aonyourwave/feedback?batch-id=b%201&streamId=s1", connection.Url);
        using var body = JsonDocument.Parse(connection.Body!);
        var root = body.RootElement;
        Assert.Equal("trackFinished", root.GetProperty("type").GetString());

        // UTC with milliseconds; a Unix timestamp sent as a string is rejected by the API.
        Assert.Equal("2024-01-02T00:04:05.678Z", root.GetProperty("timestamp").GetString());
        Assert.Equal("10:20", root.GetProperty("trackId").GetString());
        Assert.Equal("web", root.GetProperty("from").GetString());
        Assert.Equal(12.5, root.GetProperty("totalPlayedSeconds").GetDouble());
    }

    [Fact]
    public async Task StationFeedback_LeavesOutEmptyValues()
    {
        var connection = new RecordingConnection { NextResult = "ok" };

        await new RadioClient(connection).SendStationFeedbackAsync("genre:rock", "radioStarted", totalPlayedSeconds: 0);

        Assert.Equal("/rotor/station/genre%3Arock/feedback", connection.Url);
        using var body = JsonDocument.Parse(connection.Body!);
        Assert.Equal(["type", "timestamp"], body.RootElement.EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public async Task StationSettings_SendsJsonAndOmitsAnEmptyLanguage()
    {
        var connection = new RecordingConnection { NextResult = "ok" };

        await new RadioClient(connection).SetStationSettingsAsync("user:onyourwave", "fun", "discover", language: "");

        Assert.Equal("/rotor/station/user%3Aonyourwave/settings3", connection.Url);
        Assert.Equal("""{"moodEnergy":"fun","diversity":"discover","type":"rotor"}""", connection.Body);
    }

    [Fact]
    public async Task StreamFeedback_TargetsTheStreamAndParsesTheAnswer()
    {
        var connection = new RecordingConnection { NextResult = new GenerativeStreamFeedback { ReloadStream = true } };

        var result = await new RadioClient(connection).SendStationStreamFeedbackAsync("generative:focus", "streamPlay", "st-1", At);

        Assert.True(result!.ReloadStream);
        Assert.Equal("/rotor/station/generative%3Afocus/feedback?streamId=st-1", connection.Url);
        Assert.Equal("""{"type":"streamPlay","timestamp":"2024-01-02T00:04:05.678Z"}""", connection.Body);
    }

    [Fact]
    public async Task WaveEndpoints_BuildTheExpectedRequests()
    {
        var connection = new RecordingConnection();
        var radio = new RadioClient(connection);

        await radio.GetWaveSettingsAsync(["user:onyourwave", "", "settingDiversity:discover"]);
        Assert.Equal("/rotor/wave/settings?seeds=user%3Aonyourwave%2CsettingDiversity%3Adiscover", connection.Url);

        await radio.GetStationStreamAsync("generative:calm");
        Assert.Equal("/rotor/station/generative%3Acalm/stream", connection.Url);

        connection.NextResult = JsonDocument.Parse("""{"result":"ok"}""").RootElement;
        Assert.True(await radio.ResetLastWaveAsync());
        Assert.Equal(HttpMethod.Post, connection.Method);
        Assert.Equal("/rotor/wave/last/reset", connection.Url);
    }

    [Fact]
    public async Task SessionCreate_SendsSeedsAndOnlyTheGivenOptions()
    {
        var connection = new RecordingConnection();

        await new RadioSessionsClient(connection).CreateAsync(
            ["user:onyourwave", "settingDiversity:discover"],
            new RotorSessionOptions { IncludeTracksInResponse = true, Queue = ["1:2"] });

        Assert.Equal("/rotor/session/new", connection.Url);
        Assert.Equal(
            """{"seeds":["user:onyourwave","settingDiversity:discover"],"queue":["1:2"],"includeTracksInResponse":true}""",
            connection.Body);
    }

    [Fact]
    public async Task SessionTracks_AlwaysSendsTheQueueAndCarriesFeedback()
    {
        var connection = new RecordingConnection();
        var feedback = new SessionFeedback
        {
            Event = new SessionEvent { Type = "skip", Timestamp = At, TrackId = "5", TotalPlayedSeconds = 3 },
            BatchId = "b",
        };

        await new RadioSessionsClient(connection).GetTracksAsync("sess/1", feedbacks: [feedback]);

        Assert.Equal("/rotor/session/sess%2F1/tracks", connection.Url);
        Assert.Equal(
            """{"queue":[],"feedbacks":[{"event":{"type":"skip","timestamp":"2024-01-02T00:04:05.678Z","trackId":"5","totalPlayedSeconds":3},"batchId":"b"}]}""",
            connection.Body);
    }

    [Fact]
    public async Task SessionFeedback_SucceedsOnAnObjectAcknowledgement()
    {
        var connection = new RecordingConnection { NextResult = JsonDocument.Parse("{}").RootElement };
        var sessions = new RadioSessionsClient(connection);

        Assert.True(await sessions.SendTrackStartedAsync("s", "7", batchId: "b", timestamp: At));
        Assert.Equal("/rotor/session/s/feedback", connection.Url);
        Assert.Equal(
            """{"event":{"type":"trackStarted","timestamp":"2024-01-02T00:04:05.678Z","trackId":"7"},"batchId":"b"}""",
            connection.Body);

        connection.NextResult = JsonDocument.Parse("\"ok\"").RootElement;
        Assert.False(await sessions.SendRadioStartedAsync("s"));
    }

    [Fact]
    public async Task SessionsFeedbacks_GroupsEventsBySession()
    {
        var connection = new RecordingConnection { NextResult = JsonDocument.Parse("{}").RootElement };
        var feedback = new SessionFeedback
        {
            Event = new SessionEvent
            {
                Type = "playableItemStarted",
                Timestamp = At,
                Playable = new SessionPlayable { Type = "clip", Id = "9" },
            },
        };

        await new RadioSessionsClient(connection).SendSessionsFeedbacksAsync(
            [new SessionFeedbacks { SessionId = "s1", Feedbacks = [feedback] }]);

        Assert.Equal("/rotor/sessions/feedbacks", connection.Url);
        Assert.Equal(
            """{"sessions":[{"sessionId":"s1","feedbacks":[{"event":{"type":"playableItemStarted","timestamp":"2024-01-02T00:04:05.678Z","playable":{"type":"clip","id":"9"}}}]}]}""",
            connection.Body);
    }

    [Fact]
    public async Task SessionCreate_RequiresASeed()
        => await Assert.ThrowsAsync<ArgumentException>(
            () => new RadioSessionsClient(new RecordingConnection()).CreateAsync([" "]));

    [Fact]
    public void Deserializes_RotorSession()
    {
        const string json =
            """
            { "result": {
              "radioSessionId": "rs-1", "batchId": "b-1", "pumpkin": false, "terminated": false,
              "sequence": [ { "type": "track", "liked": false, "track": { "id": "1", "title": "T" },
                "trackParameters": { "bpm": 120, "energy": 0.5, "hue": 0.1, "userCollectionHue": 3,
                                     "mixFade": { "inStart": 1, "inStop": 2, "outStart": 3, "outStop": 4 } } } ],
              "acceptedSeeds": [ { "type": "user", "tag": "onyourwave" } ],
              "descriptionSeed": { "type": "genre", "tag": "rock", "value": "rock" },
              "wave": { "name": "W", "stationId": "user:onyourwave", "idForFrom": "onyourwave", "type": "DEFAULT" }
            } }
            """;

        var session = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<RotorSession>>())!.Result!;

        Assert.Equal("rs-1", session.RadioSessionId);
        var item = Assert.Single(session.Sequence!);
        Assert.Equal(120, item.TrackParameters!.Bpm);
        Assert.Equal(4, item.TrackParameters.MixFade!.OutStop);
        Assert.Equal("user:onyourwave", Assert.Single(session.AcceptedSeeds!).ToString());
        Assert.Equal("user:onyourwave", session.Wave!.StationId);
        Assert.Equal("onyourwave", session.Wave.IdForFrom);
    }

    [Fact]
    public void Deserializes_CombinedSessionItemsByType()
    {
        const string json =
            """
            { "result": { "sessionId": "c-1", "list": [
              { "type": "TRACK", "data": { "id": "1", "title": "Song" } },
              { "type": "CLIP", "data": { "clipId": 2, "title": "Video" } },
              { "type": "STORY", "data": { "id": "3" } } ] } }
            """;

        var session = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<CombinedSession>>())!.Result!;

        Assert.Equal("c-1", session.SessionId);
        Assert.Equal(3, session.Items!.Count);
        Assert.Equal("Song", session.Items[0].Track!.Title);
        Assert.Null(session.Items[0].Clip);
        Assert.Equal("Video", session.Items[1].Clip!.Title);
        Assert.Null(session.Items[2].Track);
        Assert.Null(session.Items[2].Clip);
    }

    [Fact]
    public void Deserializes_WaveSettingsWithSerializedSeeds()
    {
        const string json =
            """
            { "result": {
              "defaultStation": { "stationId": "user:1", "title": "My wave" },
              "blocks": [ { "type": "contexts", "items": [ { "name": "Work", "idForFrom": "work" } ] } ],
              "settingRestrictions": { "diversity": { "type": "enum", "name": "Diversity", "possibleValues": [
                { "value": "discover", "name": "New", "serializedSeed": "settingDiversity:discover", "imageUrl": "i" },
                { "value": "default", "name": "Any", "serializedSeed": "settingDiversity:default", "unspecified": true } ] } }
            } }
            """;

        var settings = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<WaveSettings>>())!.Result!;

        Assert.Equal("user:1", settings.DefaultStation!.StationId);
        var station = Assert.Single(Assert.Single(settings.Blocks!).Items!);
        Assert.Equal("Work", station.Name);
        Assert.Null(station.GeocellIcon);
        var values = settings.SettingRestrictions!.Diversity!.PossibleValues;
        Assert.Equal("settingDiversity:discover", values[0].SerializedSeed);
        Assert.True(values[1].Unspecified);
    }

    [Fact]
    public void Deserializes_GenerativeStream()
    {
        const string json =
            """
            { "result": {
              "data": { "title": "Focus", "backgroundColor": "#000", "derivedColors": { "accent": "#fff" } },
              "version": "v2", "stream": { "id": "st-1", "url": "https://stream" } } }
            """;

        var stream = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<GenerativeStream>>())!.Result!;

        Assert.Equal("Focus", stream.Data!.Title);
        Assert.Equal("#fff", stream.Data.DerivedColors!.Accent);
        Assert.Equal("st-1", stream.Stream!.Id);
    }
}
