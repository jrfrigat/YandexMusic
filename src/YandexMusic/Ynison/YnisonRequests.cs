namespace YandexMusic.Ynison;

/// <summary>
/// Builders for typical <see cref="PutYnisonStateRequest"/> payloads: device registration, pause and
/// resume, track switching, and volume control. They clone the server's current state and re-stamp
/// the affected versions, exactly like the reference implementations do.
/// </summary>
public static class YnisonRequests
{
    /// <summary>Generates a random device identifier, the same shape the web client uses.</summary>
    /// <returns>A random hex string without the <c>0x</c> prefix.</returns>
    public static string GenerateDeviceId() => Random.Shared.NextInt64(10_000_000_000_000_000L).ToString("x");

    /// <summary>Builds the registration request that announces this device as a remote controller.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="appName">The application name reported to other session participants.</param>
    /// <returns>The <c>UpdateFullState</c> request to send right after the state socket connects.</returns>
    public static PutYnisonStateRequest CreateUpdateFullStateRequest(string deviceId, string appName = "YandexMusic .NET")
    {
        var state = new PlayerState(
            Status: new PlayingStatus(0, 0, Paused: true, PlaybackSpeed: 1, Version: new UpdateVersion(deviceId, 0, 0)),
            PlayerQueue: new PlayerQueue(
                EntityId: "",
                EntityType: QueueEntityType.Various,
                CurrentPlayableIndex: -1,
                Options: new PlayerStateOptions(RepeatMode.None),
                Version: new UpdateVersion(deviceId, 0, 0)));

        var fullState = new UpdateFullState(
            PlayerState: state,
            IsCurrentlyActive: false,
            Device: new UpdateDevice(
                Info: new DeviceInfo(deviceId, appName, DeviceType.Web, appName),
                Capabilities: new DeviceCapabilities(CanBePlayer: false, CanBeRemoteController: true, VolumeGranularity: 0),
                VolumeInfo: new DeviceVolume(0)));

        return new PutYnisonStateRequest(
            UpdateFullState: fullState,
            PlayerActionTimestampMs: 0,
            Rid: NewRequestId());
    }

    /// <summary>Builds a pause or resume request, cloning the current playback status.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="currentStatus">The playback status from the latest server frame.</param>
    /// <param name="paused"><see langword="true"/> to pause, <see langword="false"/> to resume.</param>
    /// <returns>The <c>UpdatePlayingStatus</c> request with the flipped pause flag.</returns>
    public static PutYnisonStateRequest CreateSetPausedRequest(string deviceId, PlayingStatus currentStatus, bool paused)
    {
        ArgumentNullException.ThrowIfNull(currentStatus);

        var status = currentStatus with
        {
            Paused = paused,
            PlaybackSpeed = currentStatus.PlaybackSpeed is 0 ? 1 : currentStatus.PlaybackSpeed,
            Version = NewVersion(deviceId),
        };

        return Stamp(new PutYnisonStateRequest(UpdatePlayingStatus: new UpdatePlayingStatus(status)));
    }

    /// <summary>Builds a request switching to an adjacent track of the queue.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="currentState">The player state from the latest server frame.</param>
    /// <param name="delta">The index shift: 1 for the next track, -1 for the previous one.</param>
    /// <returns>The <c>UpdatePlayerState</c> request with the moved index and a reset progress.</returns>
    public static PutYnisonStateRequest CreateChangeTrackRequest(string deviceId, PlayerState currentState, int delta)
    {
        ArgumentNullException.ThrowIfNull(currentState);
        var queue = currentState.PlayerQueue ?? throw new ArgumentException(
            "The player state has no queue to switch tracks in.", nameof(currentState));

        var total = queue.PlayableList.Count;
        int newIndex;
        if (total == 0)
        {
            newIndex = -1;
        }
        else
        {
            var current = queue.CurrentPlayableIndex < 0 ? 0 : queue.CurrentPlayableIndex;
            newIndex = Math.Clamp(current + delta, 0, total - 1);
        }

        var paused = currentState.Status?.Paused ?? true;
        var speed = currentState.Status?.PlaybackSpeed ?? 0;
        if (speed == 0)
        {
            speed = 1;
        }
        var newState = new PlayerState(
            Status: new PlayingStatus(0, 0, paused, speed, NewVersion(deviceId)),
            PlayerQueue: queue with { CurrentPlayableIndex = newIndex, Version = NewVersion(deviceId) });

        return Stamp(new PutYnisonStateRequest(UpdatePlayerState: new UpdatePlayerState(newState)));
    }

    /// <summary>Builds a request switching to the next track of the queue.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="currentState">The player state from the latest server frame.</param>
    /// <returns>The <c>UpdatePlayerState</c> request with the index increased by one.</returns>
    public static PutYnisonStateRequest CreateNextTrackRequest(string deviceId, PlayerState currentState)
        => CreateChangeTrackRequest(deviceId, currentState, 1);

    /// <summary>Builds a request switching to the previous track of the queue.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="currentState">The player state from the latest server frame.</param>
    /// <returns>The <c>UpdatePlayerState</c> request with the index decreased by one.</returns>
    public static PutYnisonStateRequest CreatePreviousTrackRequest(string deviceId, PlayerState currentState)
        => CreateChangeTrackRequest(deviceId, currentState, -1);

    /// <summary>Builds a volume change request for a target device.</summary>
    /// <param name="deviceId">This device's identifier.</param>
    /// <param name="targetDeviceId">The device whose volume changes.</param>
    /// <param name="volume">The new volume in [0.0; 1.0]; clamped when out of range.</param>
    /// <returns>The <c>UpdateVolumeInfo</c> request.</returns>
    public static PutYnisonStateRequest CreateSetVolumeRequest(string deviceId, string targetDeviceId, double volume)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDeviceId);

        var volumeInfo = new DeviceVolume(Math.Clamp(volume, 0.0, 1.0), NewVersion(deviceId));
        return Stamp(new PutYnisonStateRequest(UpdateVolumeInfo: new UpdateVolumeInfo(targetDeviceId, volumeInfo)));
    }

    private static PutYnisonStateRequest Stamp(PutYnisonStateRequest request)
        => request with
        {
            Rid = NewRequestId(),
            PlayerActionTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

    private static string NewRequestId() => Guid.NewGuid().ToString();

    private static UpdateVersion NewVersion(string deviceId)
        => new(deviceId, Random.Shared.NextInt64(1_000_000_000_000_000_000L), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
}
