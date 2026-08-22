using YandexMusic.Exceptions;
using System.Net;
using System.Text.Json;
using YandexMusic.Quasar.Control;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Guards the local control protocol against the two things a speaker actually sends: a frame that
/// answers a command, and a frame it pushed on its own. The values are synthetic; the shape is what
/// was measured on real hardware.
/// </summary>
public sealed class QuasarControlTests
{
    private const string ReplyFrame = """
        {"id":"a1","requestId":"r1","sentTime":1787425217070,"status":"SUCCESS",
         "supported_features":["multiroom","stereo_pair"],
         "state":{"aliceState":"IDLE","canStop":true,"playing":true,"volume":0.3,
                  "playerState":{"id":"133","title":"A Title","subtitle":"A Performer",
                                 "duration":168,"progress":67.11,"playerType":"music_thin",
                                 "hasPause":true,"hasPlay":false,"hasNext":true,"hasPrev":true,
                                 "playlistId":"user:onyourwave","playlistType":"Radio",
                                 "extra":{"coverURI":"example/cover.jpg","stateType":"music"}}}}
        """;

    // What the device pushes when something changes: no status, no requestId.
    private const string PushedFrame = """
        {"id":"b2","sentTime":1787425219000,
         "state":{"playing":false,"volume":0.2,"playerState":{"title":"A Title","progress":70.5,"duration":168}}}
        """;

    private static LocalDeviceFrame Parse(string json)
        => JsonSerializer.Deserialize<LocalDeviceFrame>(json, Options())!;

    private static JsonSerializerOptions Options()
    {
        // The context is internal; InternalsVisibleTo makes it reachable, reflection keeps the test
        // from depending on its exact shape.
        var contextType = typeof(LocalDeviceFrame).Assembly
            .GetType("YandexMusic.Quasar.Control.GlagolJsonContext")!;
        var context = contextType.GetProperty("Default")!.GetValue(null)!;
        return (JsonSerializerOptions)context.GetType().GetProperty("Options")!.GetValue(context)!;
    }

    [Fact]
    public void Frame_AnsweringACommand_CarriesItsStatusAndRequestId()
    {
        var frame = Parse(ReplyFrame);

        Assert.Equal(LocalDeviceFrame.SuccessStatus, frame.Status);
        Assert.Equal("r1", frame.RequestId);
        Assert.Equal(1787425217070, frame.SentTime);
        Assert.Equal(["multiroom", "stereo_pair"], frame.SupportedFeatures);
    }

    [Fact]
    public void Frame_PushedByTheDevice_HasNoStatusAtAll()
    {
        // Most frames are these. Treating a missing status as a failure would reject nearly
        // everything the device says.
        var frame = Parse(PushedFrame);

        Assert.Null(frame.Status);
        Assert.Null(frame.RequestId);
        Assert.False(frame.State?.Playing);
    }

    [Fact]
    public void PlayerState_ReadsTheTrackAndItsPositionInSeconds()
    {
        var player = Parse(ReplyFrame).State?.PlayerState;

        Assert.NotNull(player);
        Assert.Equal("A Title", player.Title);
        Assert.Equal("A Performer", player.Subtitle);
        // Seconds, not milliseconds - the opposite of Ynison, which is the whole reason to pin it.
        Assert.Equal(168, player.Duration);
        Assert.Equal(67.11, player.Progress, precision: 2);
        Assert.Equal("user:onyourwave", player.PlaylistId);
        Assert.Equal("example/cover.jpg", player.Extra?.CoverUri);
    }

    [Fact]
    public void PlayerState_FlagsDescribeWhatIsPossibleNowNotWhatTheDeviceSupports()
    {
        // While playing, a device reports hasPause and not hasPlay. Reading these as capabilities
        // produces a play button that is disabled exactly when the user wants it.
        var player = Parse(ReplyFrame).State!.PlayerState!;

        Assert.True(player.HasPause);
        Assert.False(player.HasPlay);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public async Task SetVolumeAsync_RejectsAValueOutsideTheDeviceScale(double volume)
    {
        await using var control = new LocalDeviceControl(
            "device", new IPEndPoint(IPAddress.Loopback, 1961), "token", expectedCertificate: null);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => control.SetVolumeAsync(volume));
    }

    [Fact]
    public async Task Commands_BeforeConnectingFailWithoutTakingTheProcessDown()
    {
        await using var control = new LocalDeviceControl(
            "device", new IPEndPoint(IPAddress.Loopback, 1961), "token", expectedCertificate: null);

        await Assert.ThrowsAsync<YandexMusicQuasarException>(() => control.PauseAsync());
    }
}
