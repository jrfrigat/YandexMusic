namespace YandexMusic.Player.Playback;

/// <summary>
/// The playback "brain": owns the queue and the current track, drives the <see cref="IAudioPlayer"/>,
/// auto-advances when a track ends, and exposes simple transport controls. The UI talks only to this
/// — it never touches an <see cref="IAudioPlayer"/> directly — so adding shuffle, repeat, gapless or a
/// different sink later changes nothing above this class.
///
/// Commands arrive from two threads: the UI loop, and the audio backend signalling that a track
/// ended. Every queue change therefore runs under <see cref="_gate"/>, and the queue itself is an
/// immutable snapshot swapped by reference so readers never observe a half-applied change.
/// </summary>
public sealed class PlaybackController : IAsyncDisposable
{
    private readonly IAudioPlayer _player;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private QueueSnapshot _queue = QueueSnapshot.Empty;
    private PlaybackItem? _leftReported;

    /// <summary>Creates a controller over the given audio player.</summary>
    /// <param name="player">The audio sink to drive.</param>
    public PlaybackController(IAudioPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);
        _player = player;
        _player.StateChanged += OnPlayerStateChanged;
    }

    /// <summary>Raised when the current track or playback state changes; the UI re-renders on this.</summary>
    public event Action? Changed;

    /// <summary>Raised when a track starts playing (also on repeats and re-queues); used by play reporting.</summary>
    public event Action<PlaybackItem>? TrackStarted;

    /// <summary>Raised when the current track stops being the current one. The flag tells natural end from a user skip.</summary>
    public event Action<PlaybackItem, bool>? TrackLeft;

    /// <summary>
    /// Raised when the automatic advance to the next track fails (a dropped connection while fetching
    /// the next radio batch, an unresolvable stream). Playback has stopped by the time it fires; the
    /// UI subscribes to say why instead of leaving the player silently dead.
    /// </summary>
    public event Action<Exception>? Failed;

    /// <summary>
    /// When set, is called as the queue is about to run dry to fetch more tracks — this is how radio
    /// queues (the wave, similar tracks) keep playing forever.
    /// </summary>
    public Func<CancellationToken, Task<IReadOnlyList<PlaybackItem>>>? Continuation { get; set; }

    /// <summary>The track currently loaded, or <see langword="null"/> when the queue is empty.</summary>
    public PlaybackItem? Current => Volatile.Read(ref _queue).Current;

    /// <summary>The position of the current track within the queue (1-based), or 0 when empty.</summary>
    public int QueuePosition => Volatile.Read(ref _queue).Index + 1;

    /// <summary>The number of tracks in the queue.</summary>
    public int QueueLength => Volatile.Read(ref _queue).Items.Count;

    /// <summary>The current playback state.</summary>
    public PlaybackState State => _player.State;

    /// <summary>The current playback position.</summary>
    public TimeSpan Position => _player.Position;

    /// <summary>The duration of the current track.</summary>
    public TimeSpan Duration => _player.Duration;

    /// <summary>The output volume (0–100).</summary>
    public int Volume => _player.Volume;

    /// <summary>Whether real audio is being produced (vs a silent simulation).</summary>
    public bool ProducesSound => _player.ProducesSound;

    /// <summary>Replaces the queue and starts playing from <paramref name="startIndex"/>.</summary>
    /// <param name="items">The tracks to enqueue.</param>
    /// <param name="startIndex">The index to start from.</param>
    /// <param name="continuation">Fetches more tracks when the queue runs dry, when set.</param>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public async Task PlayAsync(
        IEnumerable<PlaybackItem> items,
        int startIndex = 0,
        Func<CancellationToken, Task<IReadOnlyList<PlaybackItem>>>? continuation = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RaiseTrackLeft(endedNaturally: false);
            var tracks = items.ToList();
            Continuation = continuation;
            _queue = new QueueSnapshot(tracks, tracks.Count == 0 ? -1 : Math.Clamp(startIndex, 0, tracks.Count - 1));
            await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _gate.Release();
        }
    }

    /// <summary>Toggles between playing and paused.</summary>
    public void TogglePause()
    {
        switch (_player.State)
        {
            case PlaybackState.Playing:
                _player.Pause();
                break;
            case PlaybackState.Paused:
                _player.Resume();
                break;
            case PlaybackState.Stopped or PlaybackState.Ended or PlaybackState.Idle when Current is not null:
                _player.Play();
                break;
        }

        Changed?.Invoke();
    }

    /// <summary>Skips to the next track, fetching a radio continuation when the queue runs dry.</summary>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public Task NextAsync(CancellationToken cancellationToken = default) => AdvanceAsync(endedNaturally: false, cancellationToken);

    /// <summary>Returns to the previous track, if any.</summary>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var queue = Volatile.Read(ref _queue);
            if (queue.Index <= 0)
            {
                return;
            }

            RaiseTrackLeft(endedNaturally: false);
            Volatile.Write(ref _queue, queue with { Index = queue.Index - 1 });
            await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _gate.Release();
        }
    }

    /// <summary>Adjusts the volume by <paramref name="delta"/> (clamped to 0–100).</summary>
    /// <param name="delta">The amount to add to the volume.</param>
    public void AdjustVolume(int delta)
    {
        _player.Volume = Math.Clamp(_player.Volume + delta, 0, 100);
        Changed?.Invoke();
    }

    /// <summary>Stops playback, leaving the queue and the current track in place.</summary>
    /// <param name="cancellationToken">A token to cancel the wait for a command already running.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RaiseTrackLeft(endedNaturally: false);
            _player.Stop();
            Changed?.Invoke();
        }
        finally
        {
            _ = _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _player.StateChanged -= OnPlayerStateChanged;
        await _player.DisposeAsync().ConfigureAwait(false);
        _gate.Dispose();
    }

    private async Task AdvanceAsync(bool endedNaturally, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var queue = Volatile.Read(ref _queue);
            if (queue.Index + 1 < queue.Items.Count)
            {
                RaiseTrackLeft(endedNaturally);
                Volatile.Write(ref _queue, queue with { Index = queue.Index + 1 });
                await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (Continuation is { } fetchMore && queue.Current is not null)
            {
                // A radio queue fetches the next batch and keeps going; on an empty batch it ends.
                var more = await fetchMore(cancellationToken).ConfigureAwait(false);
                if (more.Count > 0)
                {
                    RaiseTrackLeft(endedNaturally);
                    Volatile.Write(ref _queue, new QueueSnapshot([.. queue.Items, .. more], queue.Index + 1));
                    await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }
            }

            RaiseTrackLeft(endedNaturally: true);
            _player.Stop();
            Changed?.Invoke();
        }
        finally
        {
            _ = _gate.Release();
        }
    }

    private async Task LoadAndPlayCurrentAsync(CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _queue).Current is not { } item)
        {
            return;
        }

        await _player.LoadAsync(item, cancellationToken).ConfigureAwait(false);
        _player.Play();
        _leftReported = null;
        TrackStarted?.Invoke(item);
        Changed?.Invoke();
    }

    /// <summary>
    /// Announces that the current track is no longer current — at most once per track, so a queue
    /// that ended and is then stopped by hand does not report the same track twice.
    /// </summary>
    private void RaiseTrackLeft(bool endedNaturally)
    {
        if (Volatile.Read(ref _queue).Current is { } item && !ReferenceEquals(item, _leftReported))
        {
            _leftReported = item;
            TrackLeft?.Invoke(item, endedNaturally);
        }
    }

    private void OnPlayerStateChanged(object? sender, PlaybackState state)
    {
        if (state == PlaybackState.Ended)
        {
            // The backend signals from its own thread, so the advance cannot be awaited here. It is
            // detached but not unobserved: AutoAdvanceAsync reports whatever it fails on.
            _ = AutoAdvanceAsync();
            return;
        }

        Changed?.Invoke();
    }

    private async Task AutoAdvanceAsync()
    {
        try
        {
            await AdvanceAsync(endedNaturally: true, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _player.Stop();
            Changed?.Invoke();
            Failed?.Invoke(ex);
        }
    }

    /// <summary>The queue and the position in it, as one value so readers see a consistent pair.</summary>
    /// <param name="Items">The queued tracks.</param>
    /// <param name="Index">The index of the current track, or -1 when there is none.</param>
    private sealed record QueueSnapshot(IReadOnlyList<PlaybackItem> Items, int Index)
    {
        /// <summary>The empty queue.</summary>
        public static QueueSnapshot Empty { get; } = new([], -1);

        /// <summary>The current track, or <see langword="null"/> when the queue is empty.</summary>
        public PlaybackItem? Current => Index >= 0 && Index < Items.Count ? Items[Index] : null;
    }
}
