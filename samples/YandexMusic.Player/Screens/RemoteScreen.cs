using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic;
using YandexMusic.Exceptions;
using YandexMusic.Player.Ui;
using YandexMusic.Ynison;

namespace YandexMusic.Player.Screens;

/// <summary>
/// The Ynison remote control: a live view of the account's playback on all devices (web, phone,
/// smart speakers) with keyboard commands for pause, track switching, volume and "play on device".
/// The screen owns its Ynison session: it connects on open and disconnects on exit.
/// </summary>
public sealed class RemoteScreen
{
    private const double VolumeStep = 0.1;
    private const int MaxDeviceHotkeys = 9;

    private readonly IYandexMusicClient _client;

    /// <summary>Creates the remote screen.</summary>
    /// <param name="client">The signed-in client the Ynison session is created from.</param>
    public RemoteScreen(IYandexMusicClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
    }

    /// <summary>Runs the remote until the user presses <c>q</c>/<c>Esc</c>.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using var ynison = _client.CreateYnisonClient();
        using var sessionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var run = Task.Run(() => ynison.RunAsync(sessionCts.Token), CancellationToken.None);

        try
        {
            await ynison.WaitForStateAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
        }
        catch (YandexMusicYnisonException ex)
        {
            AnsiConsole.MarkupLine(Strings.RemoteFailed(Markup.Escape(ex.Message)));
            await StopAsync(sessionCts, run).ConfigureAwait(false);
            return;
        }

        var exit = false;
        var toast = string.Empty;
        try
        {
            await AnsiConsole.Live(Build(ynison.LatestState!, toast))
                .AutoClear(true)
                .StartAsync(async live =>
                {
                    while (!exit && !cancellationToken.IsCancellationRequested)
                    {
                        live.UpdateTarget(Build(ynison.LatestState!, toast));

                        while (TryReadKey(out var key))
                        {
                            switch (key)
                            {
                                case ConsoleKey.Spacebar or ConsoleKey.P:
                                    var paused = ynison.LatestState?.PlayerState?.Status?.Paused ?? true;
                                    await SafeAsync(() => ynison.SetPausedAsync(!paused, cancellationToken));
                                    break;
                                case ConsoleKey.RightArrow or ConsoleKey.N:
                                    await SafeAsync(() => ynison.NextTrackAsync(cancellationToken));
                                    break;
                                case ConsoleKey.LeftArrow or ConsoleKey.B:
                                    await SafeAsync(() => ynison.PreviousTrackAsync(cancellationToken));
                                    break;
                                case ConsoleKey.UpArrow or ConsoleKey.Add or ConsoleKey.OemPlus:
                                    await AdjustVolumeAsync(ynison, +VolumeStep, cancellationToken);
                                    break;
                                case ConsoleKey.DownArrow or ConsoleKey.Subtract or ConsoleKey.OemMinus:
                                    await AdjustVolumeAsync(ynison, -VolumeStep, cancellationToken);
                                    break;
                                case ConsoleKey.D1 or ConsoleKey.D2 or ConsoleKey.D3 or ConsoleKey.D4
                                    or ConsoleKey.D5 or ConsoleKey.D6 or ConsoleKey.D7 or ConsoleKey.D8
                                    or ConsoleKey.D9:
                                    toast = await PlayOnDeviceAsync(ynison, (int)key - (int)ConsoleKey.D1, cancellationToken);
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

        await StopAsync(sessionCts, run).ConfigureAwait(false);
    }

    private static async Task AdjustVolumeAsync(YnisonClient ynison, double delta, CancellationToken cancellationToken)
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

    private static async Task<string> PlayOnDeviceAsync(YnisonClient ynison, int deviceIndex, CancellationToken cancellationToken)
    {
        var state = ynison.LatestState;
        if (state is null || deviceIndex >= Math.Min(state.Devices.Count, MaxDeviceHotkeys))
        {
            return string.Empty;
        }

        var device = state.Devices[deviceIndex];
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

    private static Panel Build(PutYnisonStateResponse state, string toast)
    {
        var rows = new List<IRenderable>
        {
            new Markup(TrackLine(state)),
            new Markup(ProgressLine(state)),
            new Markup($"[grey]{Strings.RemoteDevices}[/]"),
        };

        if (state.Devices.Count == 0)
        {
            rows.Add(new Markup($"[grey]{Strings.RemoteNoDevices}[/]"));
        }

        for (var i = 0; i < Math.Min(state.Devices.Count, MaxDeviceHotkeys); i++)
        {
            rows.Add(new Markup(DeviceLine(state, state.Devices[i], i)));
        }

        rows.Add(new Markup(string.Empty));
        rows.Add(new Markup(string.IsNullOrEmpty(toast) ? Strings.RemoteKeys : ToastLine(toast)));

        return new Panel(new Rows(rows))
            .Header(Strings.RemoteHeader)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .Padding(2, 1);
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

    private static string ProgressLine(PutYnisonStateResponse state)
    {
        var status = state.PlayerState?.Status;
        var stateText = status?.Paused == true ? Strings.StatePaused : Strings.StatePlaying;
        var progress = TimeSpan.FromMilliseconds(Math.Max(0, status?.ProgressMs ?? 0));
        var duration = TimeSpan.FromMilliseconds(Math.Max(0, status?.DurationMs ?? 0));
        return $"[grey]{stateText}   {Format.Duration(progress)} / {Format.Duration(duration)}[/]";
    }

    private static string DeviceLine(PutYnisonStateResponse state, Device device, int index)
    {
        var id = device.Info?.DeviceId;
        var isActive = !string.IsNullOrEmpty(state.ActiveDeviceIdOptional) && state.ActiveDeviceIdOptional == id;
        var marker = isActive ? $" [green]{Strings.RemoteActive}[/]" : string.Empty;
        var offline = device.IsOffline ? $" [grey]({Strings.RemoteOffline})[/]" : string.Empty;
        var title = Markup.Escape(Format.Truncate(device.Info?.Title ?? Strings.RemoteUnknownDevice, 36));
        var volume = (int)Math.Round(Math.Clamp(device.VolumeInfo?.Volume ?? 0, 0, 1) * 100);
        return $"[grey][{index + 1}][/] {title}{marker}{offline}  [grey]{Strings.VolumeLabel}[/] [green]{volume,3}%[/]";
    }

    private static string ToastLine(string toast) => $"[yellow]{Markup.Escape(toast)}[/]";
}
