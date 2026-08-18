using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Ynison;

/// <summary>The answer of the redirector service: where to connect and with which ticket.</summary>
/// <param name="Host">The target Ynison host to connect the state socket to.</param>
/// <param name="RedirectTicket">The anti-DDoS ticket to pass in the subprotocols of the state socket.</param>
/// <param name="SessionId">The session id to reuse for logging and debugging.</param>
/// <param name="KeepAliveParams">The server-recommended ping interval and pong timeout.</param>
public sealed record RedirectResponse(
    string Host,
    string RedirectTicket,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long SessionId,
    KeepAliveParams? KeepAliveParams = null);

/// <summary>The keep-alive parameters recommended by the redirector.</summary>
/// <param name="KeepAliveTimeSeconds">The interval between ping frames.</param>
/// <param name="KeepAliveTimeoutSeconds">The timeout for the pong answer.</param>
public sealed record KeepAliveParams(int KeepAliveTimeSeconds = 0, int KeepAliveTimeoutSeconds = 0);

/// <summary>
/// A state update the client sends to Ynison. Exactly one of the <c>Update*</c> parameters is set
/// (a protobuf oneof); the ready-made payloads are built by <see cref="YnisonRequests"/>.
/// </summary>
/// <param name="UpdateFullState">Register/replace the full state (cold start, reconnect).</param>
/// <param name="UpdateActiveDevice">Make another device the active one.</param>
/// <param name="UpdatePlayingStatus">Change the playback status (play, pause, seek, speed).</param>
/// <param name="UpdatePlayerState">Replace the player state (new or edited queue).</param>
/// <param name="UpdateVolume">Change volume (legacy; see <paramref name="UpdateVolumeInfo"/>).</param>
/// <param name="UpdatePlayerQueueInject">Update the injected playable state.</param>
/// <param name="UpdateSessionParams">Update the server's session behaviour for this device.</param>
/// <param name="UpdateVolumeInfo">Change a device's volume.</param>
/// <param name="SyncStateFromEov">Ask to synchronize the state from the unified playback queue.</param>
/// <param name="PlayerActionTimestampMs">Unix time of the last player action, in milliseconds.</param>
/// <param name="Rid">A client-generated request id for logging and debugging.</param>
/// <param name="ActivityInterceptionType">The activity-interception tactic of the sender.</param>
public sealed record PutYnisonStateRequest(
    UpdateFullState? UpdateFullState = null,
    UpdateActiveDevice? UpdateActiveDevice = null,
    UpdatePlayingStatus? UpdatePlayingStatus = null,
    UpdatePlayerState? UpdatePlayerState = null,
    UpdateVolume? UpdateVolume = null,
    UpdatePlayerQueueInject? UpdatePlayerQueueInject = null,
    UpdateSessionParams? UpdateSessionParams = null,
    UpdateVolumeInfo? UpdateVolumeInfo = null,
    SyncStateFromEov? SyncStateFromEov = null,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long PlayerActionTimestampMs = 0,
    string Rid = "",
    ActivityInterceptionType ActivityInterceptionType = ActivityInterceptionType.DoNotInterceptByDefault);

/// <summary>
/// A state frame from Ynison: the full picture of playback and devices. Arrives in response to the
/// client's own updates, on changes made by other clients, and whenever the device list changes.
/// </summary>
/// <param name="PlayerState">The player state.</param>
/// <param name="ActiveDeviceIdOptional">The id of the device currently playing sound.</param>
/// <param name="TimestampMs">Unix time of the frame, in milliseconds.</param>
/// <param name="Rid">The request id this frame presumably answers.</param>
public sealed record PutYnisonStateResponse(
    PlayerState? PlayerState,
    string? ActiveDeviceIdOptional = null,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long TimestampMs = 0,
    string Rid = "")
{
    /// <summary>All devices of the session, including offline ones.</summary>
    public IReadOnlyList<Device> Devices { get; init; } = [];
}

/// <summary>Replace the full state: the cold-start registration of a device.</summary>
/// <param name="PlayerState">The player state.</param>
/// <param name="IsCurrentlyActive">Whether the sender is the active device.</param>
/// <param name="Device">The sender's device description.</param>
/// <param name="SyncStateFromEovOptional">An embedded unified-queue synchronization request.</param>
public sealed record UpdateFullState(
    PlayerState? PlayerState,
    bool IsCurrentlyActive = false,
    UpdateDevice? Device = null,
    SyncStateFromEov? SyncStateFromEovOptional = null);

/// <summary>Make a device the active one.</summary>
/// <param name="DeviceIdOptional">The id of the new active device.</param>
public sealed record UpdateActiveDevice(string? DeviceIdOptional = null);

/// <summary>Change the playback status.</summary>
/// <param name="PlayingStatus">The new playback status.</param>
public sealed record UpdatePlayingStatus(PlayingStatus? PlayingStatus);

/// <summary>Replace the player state.</summary>
/// <param name="PlayerState">The new player state.</param>
public sealed record UpdatePlayerState(PlayerState? PlayerState);

/// <summary>Change a device's volume (legacy).</summary>
/// <param name="Volume">The volume in [0; 1].</param>
/// <param name="DeviceId">The device whose volume changes.</param>
public sealed record UpdateVolume(double Volume, string DeviceId = "");

/// <summary>Change a device's volume.</summary>
/// <param name="DeviceId">The device whose volume changes.</param>
/// <param name="VolumeInfo">The new volume state with its version.</param>
public sealed record UpdateVolumeInfo(string DeviceId, DeviceVolume? VolumeInfo = null);

/// <summary>Update the state of an injected playable.</summary>
/// <param name="PlayerQueueInject">The injected playable state.</param>
public sealed record UpdatePlayerQueueInject(PlayerQueueInject? PlayerQueueInject);

/// <summary>Update the server's session behaviour for the sending device; resets on reconnect.</summary>
/// <param name="MuteEventsIfPassive">
/// While the device is passive it receives no events; the flag clears when the device becomes active.
/// </param>
public sealed record UpdateSessionParams(bool MuteEventsIfPassive);

/// <summary>Ask the server to synchronize the state from the unified playback queue (EOV).</summary>
/// <param name="ActualQueueId">
/// The device's queue id in the unified queue; an empty string when unknown.
/// </param>
public sealed record SyncStateFromEov(string ActualQueueId = "");
