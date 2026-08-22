using System.Net;
using YandexMusic;
using YandexMusic.Exceptions;
using YandexMusic.Quasar;
using YandexMusic.Quasar.Control;
using YandexMusicTerminal.Diagnostics;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Remote;

/// <summary>A speaker found on this network, named from the account where a name is known.</summary>
/// <param name="DeviceId">The device identifier, which both sources agree on.</param>
/// <param name="Name">What to show the user.</param>
/// <param name="Platform">The hardware model key.</param>
/// <param name="Endpoint">Where to reach it.</param>
public sealed record LocalSpeaker(string DeviceId, string Name, string Platform, IPEndPoint Endpoint);

/// <summary>
/// The remote's local half: scans the network for speakers, borrows their names from the account,
/// and holds the one connection that is being driven at a time.
///
/// Everything here is best-effort by design. A network that blocks multicast finds nothing, an
/// unreachable backend costs the speakers their friendly names but not their usability, and a failed
/// connection is a message rather than a crash — the remote's Ynison half has to keep working
/// regardless.
/// </summary>
public sealed class LocalSpeakers : IAsyncDisposable
{
    private static readonly TimeSpan ScanWindow = TimeSpan.FromSeconds(4);

    private readonly IYandexMusicClient _client;
    private readonly RequestLog _log;
    private readonly ILocalDeviceScanner _scanner = new LocalDeviceScanner();
    private readonly List<LocalSpeaker> _found = [];
    private readonly Lock _gate = new();

    private IQuasarClient? _quasar;
    private ILocalDeviceControl? _control;
    private CancellationTokenSource? _controlCts;
    private Task? _controlRun;

    /// <summary>Creates the local half of the remote.</summary>
    /// <param name="client">The signed-in client, used to reach the account's device list.</param>
    /// <param name="log">The request journal the raw device frames go to.</param>
    public LocalSpeakers(IYandexMusicClient client, RequestLog log)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(log);
        _client = client;
        _log = log;
    }

    /// <summary>The speakers found so far, in the order they answered.</summary>
    public IReadOnlyList<LocalSpeaker> Found
    {
        get
        {
            lock (_gate)
            {
                return [.. _found];
            }
        }
    }

    /// <summary>Whether a scan is running right now, so the view can say so instead of looking empty.</summary>
    public bool IsScanning { get; private set; }

    /// <summary>The speaker currently being driven, or <see langword="null"/> when none is.</summary>
    public LocalSpeaker? Connected { get; private set; }

    /// <summary>The latest state of the connected speaker.</summary>
    public LocalDeviceFrame? State => _control?.LatestState;

    /// <summary>Starts a scan in the background. Returns immediately.</summary>
    /// <param name="cancellationToken">A token to stop the scan.</param>
    public void StartScan(CancellationToken cancellationToken)
    {
        if (IsScanning)
        {
            return;
        }

        IsScanning = true;
        _ = Task.Run(() => ScanAsync(cancellationToken), CancellationToken.None);
    }

    /// <summary>Connects to a speaker and starts driving it, replacing any current connection.</summary>
    /// <param name="speaker">The speaker to drive.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A message for the user, describing what happened.</returns>
    public async Task<string> ConnectAsync(LocalSpeaker speaker, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(speaker);
        await DisconnectAsync().ConfigureAwait(false);

        try
        {
            var quasar = EnsureQuasar();
            var devices = await quasar.GetDevicesAsync(cancellationToken).ConfigureAwait(false);
            var device = devices.FirstOrDefault(d => d.Id == speaker.DeviceId)
                ?? throw new YandexMusicQuasarException(Strings.SpeakerNotOnAccount);

            // The address comes from the scan rather than the backend: mDNS answered a second ago,
            // while the backend reports wherever the device was last seen.
            var control = await quasar.ConnectAsync(device, speaker.Endpoint, cancellationToken).ConfigureAwait(false);
            control.FrameReceived += (_, frame) => _log.Write("glagol <--", frame);
            control.FrameSent += (_, frame) => _log.Write("glagol -->", frame);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var run = Task.Run(() => control.RunAsync(cts.Token), CancellationToken.None);
            await control.WaitForStateAsync(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);

            _control = control;
            _controlCts = cts;
            _controlRun = run;
            Connected = speaker;
            return Strings.SpeakerConnected(speaker.Name);
        }
        catch (Exception ex) when (ex is YandexMusicException or HttpRequestException or TimeoutException)
        {
            return Strings.SpeakerFailed(Format.Truncate(ex.Message, 70));
        }
    }

    /// <summary>Stops driving the current speaker. The speaker itself keeps playing.</summary>
    public async Task DisconnectAsync()
    {
        var control = Interlocked.Exchange(ref _control, null);
        var cts = Interlocked.Exchange(ref _controlCts, null);
        var run = Interlocked.Exchange(ref _controlRun, null);
        Connected = null;

        if (cts is not null)
        {
            await cts.CancelAsync().ConfigureAwait(false);
        }

        if (control is not null)
        {
            await control.DisposeAsync().ConfigureAwait(false);
        }

        if (run is not null)
        {
            try
            {
                await run.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // However the connection ended, leaving the screen must not take the app with it.
            }
        }

        cts?.Dispose();
    }

    /// <summary>Pauses or resumes the connected speaker.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task TogglePauseAsync(CancellationToken cancellationToken)
    {
        var control = _control;
        if (control is null)
        {
            return Task.CompletedTask;
        }

        // The device reports what is possible now, so this reads the state rather than tracking it.
        var playing = control.LatestState?.State?.Playing ?? false;
        return Safe(() => playing ? control.PauseAsync(cancellationToken) : control.PlayAsync(cancellationToken));
    }

    /// <summary>Moves the connected speaker to the next track.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task NextAsync(CancellationToken cancellationToken)
        => _control is { } control ? Safe(() => control.NextTrackAsync(cancellationToken)) : Task.CompletedTask;

    /// <summary>Moves the connected speaker back a track.</summary>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task PreviousAsync(CancellationToken cancellationToken)
        => _control is { } control ? Safe(() => control.PreviousTrackAsync(cancellationToken)) : Task.CompletedTask;

    /// <summary>Changes the connected speaker's volume by a step, clamped to the device's scale.</summary>
    /// <param name="delta">How much to change it by.</param>
    /// <param name="cancellationToken">A token to cancel the command.</param>
    public Task AdjustVolumeAsync(double delta, CancellationToken cancellationToken)
    {
        var control = _control;
        if (control is null)
        {
            return Task.CompletedTask;
        }

        var current = control.LatestState?.State?.Volume ?? 0;
        return Safe(() => control.SetVolumeAsync(Math.Clamp(current + delta, 0, 1), cancellationToken));
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _quasar?.Dispose();
        _quasar = null;
    }

    private static async Task Safe(Func<Task> command)
    {
        try
        {
            await command().ConfigureAwait(false);
        }
        catch (YandexMusicException)
        {
            // A dropped command must not kill the remote; the next state frame re-renders the truth.
        }
    }

    private IQuasarClient EnsureQuasar() => _quasar ??= _client.CreateQuasarClient();

    private async Task ScanAsync(CancellationToken cancellationToken)
    {
        try
        {
            lock (_gate)
            {
                _found.Clear();
            }

            // Names are fetched alongside the scan rather than before it: a speaker with no name yet
            // is still worth showing, and an unreachable backend must not delay the list.
            var names = NamesAsync(cancellationToken);

            await foreach (var device in _scanner.DiscoverAsync(ScanWindow, cancellationToken).ConfigureAwait(false))
            {
                lock (_gate)
                {
                    _found.Add(new LocalSpeaker(device.DeviceId, device.Platform, device.Platform, device.Endpoint));
                }
            }

            var known = await names.ConfigureAwait(false);
            lock (_gate)
            {
                for (var i = 0; i < _found.Count; i++)
                {
                    if (known.TryGetValue(_found[i].DeviceId, out var name) && !string.IsNullOrWhiteSpace(name))
                    {
                        _found[i] = _found[i] with { Name = name };
                    }
                }
            }
        }
        catch (Exception)
        {
            // Discovery finding nothing is a normal outcome on many networks, not an error to report.
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task<Dictionary<string, string>> NamesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var devices = await EnsureQuasar().GetDevicesAsync(cancellationToken).ConfigureAwait(false);
            return devices
                .GroupBy(d => d.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is YandexMusicException or HttpRequestException or InvalidOperationException)
        {
            // Without the account the speakers keep their model names, which is worse but workable.
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
