namespace YandexMusic.Quasar.Control;

/// <summary>
/// A live connection to one speaker on the local network. The device pushes its state on every
/// change once the conversation has been opened, so this is a subscription with commands attached
/// rather than a request/response API.
///
/// The usage pattern mirrors the Ynison client: start <see cref="RunAsync"/> in a background task,
/// await the first state with <see cref="WaitForStateAsync"/>, then react to
/// <see cref="StateReceived"/> and send commands. Dispose to stop.
/// </summary>
public interface ILocalDeviceControl : IAsyncDisposable
{
    /// <summary>Raised for every frame the device sends, after <see cref="LatestState"/> is updated.</summary>
    event EventHandler<LocalDeviceFrame>? StateReceived;

    /// <summary>Raised when a <see cref="StateReceived"/> handler throws, so a broken listener cannot break the connection.</summary>
    event EventHandler<Exception>? ListenerError;

    /// <summary>
    /// Raised with the raw text of every frame the device sends, before it is parsed. A diagnostic
    /// hook: for an undocumented protocol it is the only way to see what actually arrived when the
    /// parsed state looks wrong.
    /// </summary>
    event EventHandler<string>? FrameReceived;

    /// <summary>
    /// Raised with the raw text of every frame this client sends, with the device token blanked —
    /// a journal is written to be handed to somebody.
    /// </summary>
    event EventHandler<string>? FrameSent;

    /// <summary>The most recent frame, or <see langword="null"/> before the first one arrives.</summary>
    LocalDeviceFrame? LatestState { get; }

    /// <summary>The device this connection drives.</summary>
    string DeviceId { get; }

    /// <summary>
    /// Connects, opens the conversation and keeps receiving until the client is disposed, the token
    /// is cancelled, or the device closes the connection.
    /// </summary>
    /// <param name="cancellationToken">A token to stop the client.</param>
    /// <exception cref="Exceptions.YandexMusicQuasarException">
    /// The device could not be reached, or presented a certificate the backend does not vouch for.
    /// </exception>
    Task RunAsync(CancellationToken cancellationToken = default);

    /// <summary>Waits for the first frame, so commands can be built from a known state.</summary>
    /// <param name="timeout">How long to wait.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The first frame received.</returns>
    /// <exception cref="Exceptions.YandexMusicQuasarException">Nothing arrived in time, or the connection failed first.</exception>
    Task<LocalDeviceFrame> WaitForStateAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>Asks the device for its state without changing anything.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PingAsync(CancellationToken cancellationToken = default);

    /// <summary>Resumes playback.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PlayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses playback. The wire command for this is <c>stop</c>, not <c>pause</c> — a device answers
    /// <c>UNSUPPORTED</c> to the latter.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>Moves to the next track.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task NextTrackAsync(CancellationToken cancellationToken = default);

    /// <summary>Moves back to the previous track.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PreviousTrackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a specific track on the device, replacing whatever it was playing. This is how a track
    /// is handed from one player to a speaker: the speaker fetches and plays it itself, so nothing is
    /// streamed from here.
    /// </summary>
    /// <param name="trackId">The catalogue identifier of the track.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    /// <exception cref="ArgumentException"><paramref name="trackId"/> is null or whitespace.</exception>
    /// <remarks>
    /// The device answers <c>SUCCESS</c> to an empty or unknown identifier and then carries on with
    /// what it was doing, so a success here is not evidence that anything started. Watch the state.
    /// </remarks>
    Task PlayTrackAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Sets the device volume.</summary>
    /// <param name="volume">The volume, from 0.0 to 1.0.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="volume"/> is outside [0, 1].</exception>
    Task SetVolumeAsync(double volume, CancellationToken cancellationToken = default);
}
