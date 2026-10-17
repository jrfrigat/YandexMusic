using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic;
using YandexMusic.Exceptions;
using YandexMusic.Quasar.Control;
using YandexMusicTerminal.Auth;
using YandexMusicTerminal.Diagnostics;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Remote;
using YandexMusicTerminal.Ui;
using YandexMusic.Ynison;

namespace YandexMusicTerminal.Screens;

/// <summary>
/// The remote control, over both ways of reaching a speaker.
///
/// The account session (Ynison) is a live view of what is playing on every device signed in to the
/// account, and can hand playback to any of them. Beside it, this network is scanned for speakers
/// that answer directly, which is the only way to reach one the session does not list.
///
/// The transport keys drive whichever is selected, and the screen says which that is. It owns both
/// connections: they open when it opens and close when it exits.
/// </summary>
public sealed partial class RemoteScreen
{
    private const double VolumeStep = 0.1;
    private const int MaxDeviceHotkeys = 9;
    private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(4);

    private readonly IYandexMusicClient _client;
    private readonly NoticeBoard _notices;
    private readonly RequestLog _log;
    private readonly PlaybackController _controller;
    private LocalSpeakers? _speakers;
    private string _toast = string.Empty;
    private DateTime _toastShownAt;

    /// <summary>Creates the remote screen.</summary>
    /// <param name="client">The signed-in client the Ynison session is created from.</param>
    /// <param name="notices">The board a failed connection reports to.</param>
    /// <param name="log">The request journal the raw Ynison frames go to.</param>
    /// <param name="controller">The player's own playback, so its track can be handed to a speaker.</param>
    public RemoteScreen(IYandexMusicClient client, NoticeBoard notices, RequestLog log, PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(controller);
        _client = client;
        _notices = notices;
        _log = log;
        _controller = controller;
    }

    /// <summary>Runs the remote until the user presses <c>q</c>/<c>Esc</c>.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        // A stable id and a name that says which machine this is: the account's device list keeps
        // every id that ever registered, so a per-run id would litter it with dead entries.
        await using var ynison = _client.CreateYnisonClient(
            DeviceIdentity.GetOrCreate(),
            new YnisonClientOptions { AppName = DeviceIdentity.DisplayName() });

        // The device list lives in these frames; when it looks wrong, the raw text is the evidence.
        ynison.FrameReceived += (_, frame) => _log.Write("ynison <--", frame);
        ynison.FrameSent += (_, frame) => _log.Write("ynison -->", frame);

        // A dropped connection or a rejected request is recovered from on its own; saying so keeps
        // a remote that briefly stops reacting from looking frozen.
        ynison.ConnectionError += (_, error) => ShowToast(Strings.RemoteReconnecting(Format.Truncate(error.Message, 60)));

        using var sessionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var run = Task.Run(() => ynison.RunAsync(sessionCts.Token), CancellationToken.None);

        // The two halves are independent: speakers on this network answer with no account involved,
        // and the scan runs beside the Ynison handshake rather than after it.
        await using var speakers = new LocalSpeakers(_client, _log);
        _speakers = speakers;
        speakers.StartScan(sessionCts.Token);

        try
        {
            await ynison.WaitForStateAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        }
        catch (YandexMusicYnisonException ex)
        {
            _notices.Post(Strings.RemoteFailed(ex.Message));
            await StopAsync(sessionCts, run).ConfigureAwait(false);
            return;
        }

        var exit = false;
        try
        {
            await AnsiConsole.Live(Build(ynison.LatestState!, ynison.TimeSinceLatestState))
                .AutoClear(true)
                .StartAsync(async live =>
                {
                    while (!exit && !cancellationToken.IsCancellationRequested)
                    {
                        // The session ended for good (the token was rejected, another client took
                        // over this device id, or the server asked to stay away): there is nothing
                        // left to drive, so leave and say why instead of showing a frozen panel.
                        if (run.IsCompleted)
                        {
                            _notices.Post(Strings.RemoteFailed(run.Exception?.GetBaseException().Message ?? Strings.RemoteSessionClosed));
                            exit = true;
                            break;
                        }

                        live.UpdateTarget(Build(ynison.LatestState!, ynison.TimeSinceLatestState));

                        while (TryReadKey(out var key))
                        {
                            // Transport keys act on whatever is being driven: the account's session
                            // by default, or one speaker on this network once it is selected.
                            var local = speakers.Connected is not null;
                            switch (key)
                            {
                                case ConsoleKey.Spacebar or ConsoleKey.P:
                                    if (local)
                                    {
                                        await speakers.TogglePauseAsync(cancellationToken).ConfigureAwait(false);
                                        break;
                                    }

                                    var paused = ynison.LatestState?.PlayerState?.Status?.Paused ?? true;
                                    await SafeAsync(() => ynison.SetPausedAsync(!paused, cancellationToken));
                                    break;
                                case ConsoleKey.RightArrow or ConsoleKey.N:
                                    await (local
                                        ? speakers.NextAsync(cancellationToken)
                                        : StepAsync(ynison, +1, cancellationToken)).ConfigureAwait(false);
                                    break;
                                case ConsoleKey.LeftArrow or ConsoleKey.B:
                                    await (local
                                        ? speakers.PreviousAsync(cancellationToken)
                                        : StepAsync(ynison, -1, cancellationToken)).ConfigureAwait(false);
                                    break;
                                case ConsoleKey.UpArrow or ConsoleKey.Add or ConsoleKey.OemPlus:
                                    await (local
                                        ? speakers.AdjustVolumeAsync(+VolumeStep, cancellationToken)
                                        : AdjustVolumeAsync(ynison, +VolumeStep, cancellationToken)).ConfigureAwait(false);
                                    break;
                                case ConsoleKey.DownArrow or ConsoleKey.Subtract or ConsoleKey.OemMinus:
                                    await (local
                                        ? speakers.AdjustVolumeAsync(-VolumeStep, cancellationToken)
                                        : AdjustVolumeAsync(ynison, -VolumeStep, cancellationToken)).ConfigureAwait(false);
                                    break;
                                case ConsoleKey.D1 or ConsoleKey.D2 or ConsoleKey.D3 or ConsoleKey.D4
                                    or ConsoleKey.D5 or ConsoleKey.D6 or ConsoleKey.D7 or ConsoleKey.D8
                                    or ConsoleKey.D9:
                                    ShowToast(await SelectAsync(ynison, speakers, (int)key - (int)ConsoleKey.D1, cancellationToken));
                                    break;
                                case ConsoleKey.D0:
                                    await speakers.DisconnectAsync().ConfigureAwait(false);
                                    ShowToast(Strings.SpeakerBackToSession);
                                    break;
                                case ConsoleKey.T:
                                    ShowToast(await HandOverAsync(speakers, cancellationToken).ConfigureAwait(false));
                                    break;
                                case ConsoleKey.R:
                                    speakers.StartScan(cancellationToken);
                                    break;
                                case ConsoleKey.Q or ConsoleKey.Escape:
                                    exit = true;
                                    break;
                            }
                        }

                        await Task.Delay(150, cancellationToken).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ctrl+C — exit quietly.
        }

        _speakers = null;
        await StopAsync(sessionCts, run).ConfigureAwait(false);
    }

    private static async Task AdjustVolumeAsync(IYnisonClient ynison, double delta, CancellationToken cancellationToken)
    {
        var state = ynison.LatestState;
        var activeId = state?.ActiveDeviceIdOptional;
        if (state is null || string.IsNullOrEmpty(activeId))
        {
            return;
        }

        var device = FindDevice(state, activeId);
        var current = device?.VolumeInfo?.Volume ?? 0;
        await SafeAsync(() => ynison.SetVolumeAsync(activeId, current + delta, cancellationToken));
    }

    /// <summary>
    /// The devices a hotkey may target: the ones that said they can play sound. The session also
    /// lists pure remote controllers (other copies of this player, stale registrations from earlier
    /// runs), and telling one of those to "play here" asks for something it cannot do.
    /// </summary>
    private static List<Device> PlayableDevices(PutYnisonStateResponse state)
        => [.. state.Devices.Where(d => d.Capabilities?.CanBePlayer == true).Take(MaxDeviceHotkeys)];

    /// <summary>
    /// Hands the track the player itself is on to the selected speaker. The speaker fetches and plays
    /// it from the catalogue, so nothing is streamed from here and the player is free to stop.
    /// </summary>
    /// <remarks>
    /// Only a speaker can be handed a specific track. The account session can be told which device
    /// should play, but not what — that needs a queue, which this protocol does not expose here.
    /// </remarks>
    private async Task<string> HandOverAsync(LocalSpeakers speakers, CancellationToken cancellationToken)
    {
        if (speakers.Connected is not { } speaker)
        {
            return Strings.HandOverNeedsSpeaker;
        }

        if (_controller.Current is not { } track)
        {
            return Strings.HandOverNothingPlaying;
        }

        var failure = await speakers.PlayTrackAsync(track.Id, cancellationToken).ConfigureAwait(false);
        return failure ?? Strings.HandOverSent(Format.Truncate(track.Title, 34), Format.Truncate(speaker.Name, 24));
    }

    /// <summary>
    /// The speakers a hotkey may target, numbered after the Ynison devices so one row of numbers
    /// covers both lists.
    /// </summary>
    private static List<LocalSpeaker> HotkeySpeakers(LocalSpeakers speakers, int alreadyUsed)
        => [.. speakers.Found.Take(Math.Max(0, MaxDeviceHotkeys - alreadyUsed))];

    /// <summary>
    /// Acts on a number key. A Ynison device is told to start playing; a speaker on this network is
    /// connected to instead, because there is no session to hand it — the remote drives it directly.
    /// </summary>
    private static async Task<string> SelectAsync(
        IYnisonClient ynison,
        LocalSpeakers speakers,
        int index,
        CancellationToken cancellationToken)
    {
        var playable = ynison.LatestState is { } state ? PlayableDevices(state) : [];
        if (index < playable.Count)
        {
            // Picking a session device hands the keys back to the session. Without this the remote
            // starts playing on the phone while pause and the arrows still go to a speaker nobody
            // was looking at — which reads as the remote having broken.
            await speakers.DisconnectAsync().ConfigureAwait(false);
            return await PlayOnDeviceAsync(ynison, index, cancellationToken).ConfigureAwait(false);
        }

        var local = HotkeySpeakers(speakers, playable.Count);
        var localIndex = index - playable.Count;
        if (localIndex >= local.Count)
        {
            return string.Empty;
        }

        return await speakers.ConnectAsync(local[localIndex], cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> PlayOnDeviceAsync(IYnisonClient ynison, int deviceIndex, CancellationToken cancellationToken)
    {
        var state = ynison.LatestState;
        if (state is null)
        {
            return string.Empty;
        }

        var playable = PlayableDevices(state);
        if (deviceIndex >= playable.Count)
        {
            return string.Empty;
        }

        var device = playable[deviceIndex];
        var title = device.Info?.Title ?? Strings.RemoteUnknownDevice;
        try
        {
            await ynison.PlayOnDeviceAsync(device.Info?.DeviceId ?? string.Empty, cancellationToken).ConfigureAwait(false);
            return Strings.RemoteStartedOn(Format.Truncate(title, 40));
        }
        catch (YandexMusicException ex)
        {
            return Strings.RemoteCommandFailed(Format.Truncate(ex.Message, 60));
        }
    }

    /// <summary>
    /// Switches the session's track in playback order (shuffle included). At the edge of the queue
    /// nothing is sent; the user is told instead, rather than the current track silently restarting.
    /// </summary>
    private Task StepAsync(IYnisonClient ynison, int delta, CancellationToken cancellationToken)
    {
        if (ynison.LatestState?.PlayerState?.PlayerQueue is { } queue && YnisonQueueNavigation.GetNeighbourIndex(queue, delta) is null)
        {
            ShowToast(delta > 0 ? Strings.RemoteQueueEnd : Strings.RemoteQueueStart);
            return Task.CompletedTask;
        }

        return SafeAsync(() => delta > 0 ? ynison.NextTrackAsync(cancellationToken) : ynison.PreviousTrackAsync(cancellationToken));
    }

    private static async Task SafeAsync(Func<Task> command)
    {
        try
        {
            await command().ConfigureAwait(false);
        }
        catch (YandexMusicException)
        {
            // A dropped command must not kill the remote; the next frame re-renders the truth.
        }
    }

    private static async Task StopAsync(CancellationTokenSource sessionCts, Task run)
    {
        sessionCts.Cancel();
        try
        {
            await run.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Whatever the session died of, closing the remote must never exit the app.
        }
    }

    private static Device? FindDevice(PutYnisonStateResponse state, string deviceId)
        => state.Devices.FirstOrDefault(d => d.Info?.DeviceId == deviceId);

    private static bool TryReadKey(out ConsoleKey key)
    {
        key = default;
        try
        {
            if (!Console.KeyAvailable)
            {
                return false;
            }

            key = Console.ReadKey(intercept: true).Key;
            return true;
        }
        catch (InvalidOperationException)
        {
            // Input is redirected — no interactive keys.
            return false;
        }
    }

    private void ShowToast(string message)
    {
        _toast = message;
        _toastShownAt = DateTime.UtcNow;
    }
}
