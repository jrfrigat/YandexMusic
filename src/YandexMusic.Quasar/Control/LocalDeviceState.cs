using System.Text.Json.Serialization;

namespace YandexMusic.Quasar.Control;

/// <summary>
/// What a speaker reports about itself. Every field is what the device sent, unmodified.
///
/// Frames come in two kinds and it matters which one is in hand. A frame that answers a command
/// carries <see cref="Status"/> and <see cref="RequestId"/>; a frame the device pushed on its own —
/// which is most of them, since it announces every change — carries neither.
/// </summary>
/// <param name="Status">
/// The outcome of the request this frame answers: <c>SUCCESS</c>, or <c>UNSUPPORTED</c> for a
/// command the device does not know. <see langword="null"/> on a frame the device pushed by itself.
/// </param>
/// <param name="RequestId">The <c>id</c> of the request this frame answers, or <see langword="null"/> when it answers none.</param>
/// <param name="SentTime">When the device sent this frame, as a Unix time in milliseconds.</param>
/// <param name="State">The device's state.</param>
/// <remarks>
/// A reply's <see cref="State"/> is the state from <b>before</b> the command was applied. Confirming
/// a command by reading its own answer therefore concludes, every time, that nothing happened; watch
/// the frames that follow instead.
/// </remarks>
public sealed record LocalDeviceFrame(
    string? Status,
    string? RequestId,
    long SentTime,
    LocalDeviceStatus? State)
{
    /// <summary>The status a device returns for a command it does not know.</summary>
    public const string UnsupportedStatus = "UNSUPPORTED";

    /// <summary>The status a device returns for a command it carried out.</summary>
    public const string SuccessStatus = "SUCCESS";

    /// <summary>What the device can do, as a list of capability names such as <c>multiroom</c>.</summary>
    [JsonPropertyName("supported_features")]
    public IReadOnlyList<string> SupportedFeatures { get; init; } = [];
}

/// <summary>A speaker's current state.</summary>
/// <param name="Playing">Whether audio is playing right now.</param>
/// <param name="Volume">The volume, from 0.0 to 1.0.</param>
/// <param name="AliceState">What the assistant is doing, for example <c>IDLE</c> or <c>LISTENING</c>.</param>
/// <param name="CanStop">Whether there is something to stop.</param>
/// <param name="PlayerState">What is playing, when anything is.</param>
public sealed record LocalDeviceStatus(
    bool Playing,
    double Volume,
    string? AliceState,
    bool CanStop,
    LocalPlayerState? PlayerState);

/// <summary>What a speaker is playing.</summary>
/// <param name="Id">The track identifier.</param>
/// <param name="Title">The track title.</param>
/// <param name="Subtitle">The performer.</param>
/// <param name="Duration">
/// The track's length in <b>seconds</b>. Ynison reports milliseconds for the same thing, so anything
/// showing both sources on one progress bar has to convert.
/// </param>
/// <param name="Progress">How far into the track playback is, in seconds, fractional.</param>
/// <param name="PlaylistId">The playlist or station being played, for example <c>user:onyourwave</c>.</param>
/// <param name="PlaylistType">The kind of thing being played, for example <c>Radio</c>.</param>
/// <param name="PlayerType">The playback engine in use, for example <c>music_thin</c>.</param>
/// <param name="Extra">Loose additional values, including <c>coverURI</c>.</param>
public sealed record LocalPlayerState(
    string? Id,
    string? Title,
    string? Subtitle,
    double Duration,
    double Progress,
    string? PlaylistId,
    string? PlaylistType,
    string? PlayerType,
    LocalPlayerExtra? Extra)
{
    /// <summary>
    /// Whether resuming is possible <b>right now</b>. This is not a capability: while the device is
    /// playing it reports <see langword="false"/> here and <see langword="true"/> for
    /// <see cref="HasPause"/>. Reading it as "this device can play" produces a permanently disabled
    /// button.
    /// </summary>
    public bool HasPlay { get; init; }

    /// <summary>Whether pausing is possible right now. See the note on <see cref="HasPlay"/>.</summary>
    public bool HasPause { get; init; }

    /// <summary>Whether there is a next track to move to right now.</summary>
    public bool HasNext { get; init; }

    /// <summary>Whether there is a previous track to move back to right now.</summary>
    public bool HasPrev { get; init; }

    /// <summary>Whether a position is meaningful for what is playing; false for a live stream.</summary>
    public bool HasProgressBar { get; init; }
}

/// <summary>The loose extras a speaker attaches to its player state.</summary>
/// <param name="CoverUri">The cover art location, without a scheme or size suffix.</param>
/// <param name="StateType">What kind of thing is playing, for example <c>music</c>.</param>
public sealed record LocalPlayerExtra(
    [property: JsonPropertyName("coverURI")] string? CoverUri,
    string? StateType);
