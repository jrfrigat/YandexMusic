using Spectre.Console;
using Spectre.Console.Rendering;
using YandexMusic.Quasar.Control;
using YandexMusic.Ynison;
using YandexMusicTerminal.Remote;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>The rendering half of the remote: the panel, the track line, the progress and the device list.</summary>
public sealed partial class RemoteScreen
{
    private Panel Build(PutYnisonStateResponse state, TimeSpan sinceFrame)
    {
        // The headline follows the target. Showing the session's track while the keys are driving a
        // speaker is what makes the remote look frozen: the speaker pauses, and the big line above
        // it — which belongs to a different device entirely — does not move.
        var driving = _speakers?.Connected is not null ? _speakers.State : null;

        var rows = new List<IRenderable>
        {
            new Markup(driving is null ? TrackLine(state) : LocalTrackLine(driving)),
            new Markup(driving is null ? ProgressLine(state, sinceFrame) : LocalProgressLine(driving)),
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

        var volume = (int)Math.Round(Math.Clamp(_speakers.State?.State?.Volume ?? 0, 0, 1) * 100);
        return $"[cyan]{Markup.Escape(Strings.RemoteTargetSpeaker(Format.Truncate(speaker.Name, 34)))}[/]" +
               $"  [grey]{Strings.VolumeLabel}[/] [green]{volume,3}%[/]";
    }

    /// <summary>The track a driven speaker is on, in the same place the session's track otherwise sits.</summary>
    private static string LocalTrackLine(LocalDeviceFrame frame)
    {
        var title = frame.State?.PlayerState?.Title;
        if (string.IsNullOrWhiteSpace(title))
        {
            return $"[grey]{Strings.RemoteNothing}[/]";
        }

        var performer = frame.State?.PlayerState?.Subtitle;
        var artist = string.IsNullOrWhiteSpace(performer)
            ? string.Empty
            : $" [grey]— {Markup.Escape(Format.Truncate(performer, 30))}[/]";

        return $"[bold white]{Markup.Escape(Format.Truncate(title, 42))}[/]{artist}";
    }

    /// <summary>
    /// A driven speaker's position. Unlike Ynison this needs no extrapolating: the device pushes a
    /// frame about once a second while it plays. The times are seconds here, not milliseconds.
    /// </summary>
    private static string LocalProgressLine(LocalDeviceFrame frame)
    {
        var player = frame.State?.PlayerState;
        var stateText = frame.State?.Playing == true ? Strings.StatePlaying : Strings.StatePaused;
        var progress = TimeSpan.FromSeconds(Math.Max(0, player?.Progress ?? 0));
        var duration = TimeSpan.FromSeconds(Math.Max(0, player?.Duration ?? 0));
        return $"[grey]{stateText}   {Format.Duration(progress)} / {Format.Duration(duration)}[/]";
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
