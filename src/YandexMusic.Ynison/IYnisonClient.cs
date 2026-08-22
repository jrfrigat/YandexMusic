namespace YandexMusic.Ynison;

/// <summary>
/// A Ynison session: a long-lived websocket subscription to the account's playback state across all
/// its devices, and a channel for remote-control commands. Ynison is what synchronizes the web
/// player, the phone apps and the smart speakers, so an implementation can observe what is playing
/// anywhere and control it.
///
/// The usage pattern is: start <see cref="RunAsync"/> in a background task, await the first frame
/// with <see cref="WaitForStateAsync"/>, then either react to <see cref="StateReceived"/> or send
/// commands. Dispose to stop.
/// </summary>
public interface IYnisonClient : IAsyncDisposable
{
    /// <summary>Raised for every state frame received, after <see cref="LatestState"/> was updated.</summary>
    event EventHandler<PutYnisonStateResponse>? StateReceived;

    /// <summary>Raised when a <see cref="StateReceived"/> handler throws, so a broken listener cannot break the session.</summary>
    event EventHandler<Exception>? ListenerError;

    /// <summary>
    /// Raised with the raw text of every frame the server sends, before it is parsed. A diagnostic
    /// hook: it is the only way to see what actually arrived when the parsed state looks wrong.
    /// The payload carries no credentials — the token travels in the connection headers.
    /// </summary>
    event EventHandler<string>? FrameReceived;

    /// <summary>Raised with the raw text of every frame this client sends. The diagnostic counterpart of <see cref="FrameReceived"/>.</summary>
    event EventHandler<string>? FrameSent;

    /// <summary>This client's identifier in the Ynison session.</summary>
    string DeviceId { get; }

    /// <summary>The most recent state frame, or <see langword="null"/> before the first one arrives.</summary>
    PutYnisonStateResponse? LatestState { get; }

    /// <summary>
    /// How long ago <see cref="LatestState"/> arrived, or <see cref="TimeSpan.Zero"/> before the
    /// first frame. Ynison pushes a frame only when something changes, never a progress tick, so a
    /// caller showing or sending a playback position must add this to the frame's position.
    /// </summary>
    TimeSpan TimeSinceLatestState { get; }

    /// <summary>Runs the connection until the client is disposed, the token is cancelled, or the server closes the session.</summary>
    /// <param name="cancellationToken">A token to stop the client.</param>
    Task RunAsync(CancellationToken cancellationToken = default);

    /// <summary>Waits for the first state frame, so commands can be built from a known state.</summary>
    /// <param name="timeout">How long to wait for the frame.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The first state frame.</returns>
    /// <exception cref="Exceptions.YandexMusicYnisonException">No frame arrived in time, or the connection failed first.</exception>
    Task<PutYnisonStateResponse> WaitForStateAsync(TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>Sends an arbitrary state update, normally built by <see cref="YnisonRequests"/>.</summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the send.</param>
    /// <exception cref="Exceptions.YandexMusicYnisonException">The socket is not connected, or the send failed.</exception>
    Task SendAsync(PutYnisonStateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Pauses or resumes playback on the active device.</summary>
    /// <param name="paused"><see langword="true"/> to pause, <see langword="false"/> to resume.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task SetPausedAsync(bool paused, CancellationToken cancellationToken = default);

    /// <summary>Switches the active device to the next track of its queue.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task NextTrackAsync(CancellationToken cancellationToken = default);

    /// <summary>Switches the active device to the previous track of its queue.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PreviousTrackAsync(CancellationToken cancellationToken = default);

    /// <summary>Changes the volume of a device of the session.</summary>
    /// <param name="targetDeviceId">The device whose volume changes.</param>
    /// <param name="volume">The new volume in [0.0; 1.0].</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task SetVolumeAsync(string targetDeviceId, double volume, CancellationToken cancellationToken = default);

    /// <summary>Makes another device the active one; it takes over the session's playback.</summary>
    /// <param name="targetDeviceId">The device that should play the sound.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task SetActiveDeviceAsync(string targetDeviceId, CancellationToken cancellationToken = default);

    /// <summary>Starts playback of the current track on a device: makes it active and resumes.</summary>
    /// <param name="targetDeviceId">The device to start playback on.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    Task PlayOnDeviceAsync(string targetDeviceId, CancellationToken cancellationToken = default);
}
