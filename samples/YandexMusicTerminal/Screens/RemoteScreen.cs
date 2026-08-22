using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic;
using YandexMusic.Exceptions;
using YandexMusicTerminal.Auth;
using YandexMusicTerminal.Diagnostics;
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
public sealed class RemoteScreen
{
    private const double VolumeStep = 0.1;
    private const int MaxDeviceHotkeys = 9;
    private static readonly TimeSpan ToastLifetime = TimeSpan.FromSeconds(4);

    private readonly IYandexMusicClient _client;
    private readonly NoticeBoard _notices;
    private readonly RequestLog _log;
    private LocalSpeakers? _speakers;
    private string _toast = string.Empty;
    private DateTime _toastShownAt;

    /// <summary>Creates the remote screen.</summary>
    /// <param name="client">The signed-in client the Ynison session is created from.</param>
    /// <param name="notices">The board a failed connection reports to.</param>
    /// <param name="log">The request journal the raw Ynison frames go to.</param>
    public RemoteScreen(IYandexMusicClient client, NoticeBoard notices, RequestLog log)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentNullException.ThrowIfNull(log);
        _client = client;
        _notices = notices;
        _log = log;
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
                                        : SafeAsync(() => ynison.NextTrackAsync(cancellationToken))).ConfigureAwait(false);
                                    break;
                                case ConsoleKey.LeftArrow or ConsoleKey.B:
                                    await (local
                                        ? speakers.PreviousAsync(cancellationToken)
                                        : SafeAsync(() => ynison.PreviousTrackAsync(cancellationToken))).ConfigureAwait(false);
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

    private Panel Build(PutYnisonStateResponse state, TimeSpan sinceFrame)
    {
        var rows = new List<IRenderable>
        {
            new Markup(TrackLine(state)),
            new Markup(ProgressLine(state, sinceFrame)),
            new Markup(TargetLine()),
            new Markup($"[grey]{Strings.RemoteDevices}[/]"),
        };

        if (state.Devices.Count == 0)
        {
            rows.Add(new Markup($"[grey]{Strings.RemoteNoDevices}[/]"));
        }

        // Hotkeys are numbered over the playable devices only, so the number next to a device is the
        // key that actually starts playback on it. Everything else is listed without one.
        var playable = PlayableDevices(state);
        foreach (var device in state.Devices)
        {
            var hotkey = playable.IndexOf(device);
            rows.Add(new Markup(DeviceLine(state, device, hotkey)));
        }

        AppendSpeakers(rows, playable.Count);

        rows.Add(new Markup(string.Empty));
        if (!string.IsNullOrEmpty(_toast) && DateTime.UtcNow - _toastShownAt < ToastLifetime)
        {
            rows.Add(new Markup($"[grey]{Markup.Escape(_toast)}[/]"));
        }
        else
        {
            rows.Add(new Markup(Strings.RemoteKeys));
        }

        return new Panel(new Rows(rows))
            .Header(Strings.RemoteHeader)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Padding(2, 1);
    }

    /// <summary>Says what the transport keys are driving right now, because the same keys do both.</summary>
    private string TargetLine()
    {
        if (_speakers?.Connected is not { } speaker)
        {
            return $"[grey]{Strings.RemoteTargetSession}[/]";
        }

        var player = _speakers.State?.State;
        var mark = player?.Playing == true ? "[green]>[/]" : "[yellow]||[/]";
        var volume = (int)Math.Round(Math.Clamp(player?.Volume ?? 0, 0, 1) * 100);
        var title = player?.PlayerState?.Title is { Length: > 0 } name
            ? $"  [white]{Markup.Escape(Format.Truncate(name, 34))}[/]"
            : string.Empty;

        return $"[cyan]{Markup.Escape(Strings.RemoteTargetSpeaker(Format.Truncate(speaker.Name, 28)))}[/] " +
               $"{mark}{title}  [grey]{Strings.VolumeLabel}[/] [green]{volume,3}%[/]";
    }

    /// <summary>Renders the speakers found on this network, numbered on from the Ynison devices.</summary>
    private void AppendSpeakers(List<IRenderable> rows, int hotkeysUsed)
    {
        if (_speakers is not { } speakers)
        {
            return;
        }

        rows.Add(new Markup(string.Empty));
        rows.Add(new Markup($"[grey]{Strings.RemoteLocalSection}[/]"));

        var found = HotkeySpeakers(speakers, hotkeysUsed);
        if (found.Count == 0)
        {
            rows.Add(new Markup(speakers.IsScanning
                ? $"[grey]{Strings.RemoteLocalScanning}[/]"
                : $"[grey]{Strings.RemoteLocalNone}[/]"));
            return;
        }

        for (var i = 0; i < found.Count; i++)
        {
            var speaker = found[i];
            var connected = speakers.Connected?.DeviceId == speaker.DeviceId;
            var badge = $"[grey][[{hotkeysUsed + i + 1}]] [/]";
            var name = Markup.Escape(Format.Truncate(speaker.Name, 36));
            var mark = connected ? $" [cyan]{Strings.RemoteLocalDriving}[/]" : string.Empty;
            var model = $"  [grey]{Markup.Escape(speaker.Platform)}[/]";
            rows.Add(new Markup($"{badge}{name}{mark}{model}"));
        }
    }

    private static string TrackLine(PutYnisonStateResponse state)
    {
        var queue = state.PlayerState?.PlayerQueue;
        var index = queue?.CurrentPlayableIndex ?? -1;
        var playable = queue is not null && index >= 0 && index < queue.PlayableList.Count
            ? queue.PlayableList[index]
            : null;
        return playable is null
            ? $"[grey]{Strings.RemoteNothing}[/]"
            : $"[bold white]{Markup.Escape(Format.Truncate(playable.Title, 60))}[/]";
    }

    /// <summary>
    /// The playback position, advanced by however long ago the frame arrived. Ynison sends a frame
    /// only when something changes, so rendering its position verbatim shows a counter frozen on the
    /// second the last change happened.
    /// </summary>
    private static string ProgressLine(PutYnisonStateResponse state, TimeSpan sinceFrame)
    {
        var status = state.PlayerState?.Status;
        var playing = status?.Paused == false;
        var stateText = playing ? Strings.StatePlaying : Strings.StatePaused;
        var elapsed = playing ? (long)sinceFrame.TotalMilliseconds : 0;
        var durationMs = Math.Max(0, status?.DurationMs ?? 0);
        var progressMs = Math.Max(0, status?.ProgressMs ?? 0) + elapsed;
        if (durationMs > 0)
        {
            progressMs = Math.Min(progressMs, durationMs);
        }

        var progress = TimeSpan.FromMilliseconds(progressMs);
        var duration = TimeSpan.FromMilliseconds(durationMs);
        return $"[grey]{stateText}   {Format.Duration(progress)} / {Format.Duration(duration)}[/]";
    }

    /// <summary>Renders one device row; <paramref name="hotkey"/> is -1 for a device no key targets.</summary>
    private static string DeviceLine(PutYnisonStateResponse state, Device device, int hotkey)
    {
        var id = device.Info?.DeviceId;
        var isActive = !string.IsNullOrEmpty(state.ActiveDeviceIdOptional) && state.ActiveDeviceIdOptional == id;
        var marker = isActive ? $" [green]{Strings.RemoteActive}[/]" : string.Empty;
        var offline = device.IsOffline ? $" [grey]({Strings.RemoteOffline})[/]" : string.Empty;
        var title = Markup.Escape(Format.Truncate(device.Info?.Title ?? Strings.RemoteUnknownDevice, 36));
        var volume = (int)Math.Round(Math.Clamp(device.VolumeInfo?.Volume ?? 0, 0, 1) * 100);
        var badge = hotkey >= 0 ? $"[grey][[{hotkey + 1}]] [/]" : "[grey]    [/]";
        return $"{badge}{title}{marker}{offline}  [grey]{Strings.VolumeLabel}[/] [green]{volume,3}%[/]";
    }

}
