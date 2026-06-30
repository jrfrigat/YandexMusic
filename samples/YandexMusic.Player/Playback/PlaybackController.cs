namespace YandexMusic.Player.Playback;

/// <summary>
/// The playback "brain": owns the queue and the current track, drives the <see cref="IAudioPlayer"/>,
/// auto-advances when a track ends, and exposes simple transport controls. The UI talks only to this
/// — it never touches an <see cref="IAudioPlayer"/> directly — so adding shuffle, repeat, gapless or a
/// different sink later changes nothing above this class.
/// </summary>
public sealed class PlaybackController : IAsyncDisposable
{
    private readonly IAudioPlayer _player;
    private readonly List<PlaybackItem> _queue = [];
    private int _index = -1;

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

    /// <summary>The track currently loaded, or <see langword="null"/> when the queue is empty.</summary>
    public PlaybackItem? Current => _index >= 0 && _index < _queue.Count ? _queue[_index] : null;

    /// <summary>The position of the current track within the queue (1-based), or 0 when empty.</summary>
    public int QueuePosition => _index < 0 ? 0 : _index + 1;

    /// <summary>The number of tracks in the queue.</summary>
    public int QueueLength => _queue.Count;

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
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public async Task PlayAsync(IEnumerable<PlaybackItem> items, int startIndex = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        _queue.Clear();
        _queue.AddRange(items);
        _index = _queue.Count == 0 ? -1 : Math.Clamp(startIndex, 0, _queue.Count - 1);
        await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
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

    /// <summary>Skips to the next track, if any.</summary>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public async Task NextAsync(CancellationToken cancellationToken = default)
    {
        if (_index + 1 < _queue.Count)
        {
            _index++;
            await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _player.Stop();
            Changed?.Invoke();
        }
    }

    /// <summary>Returns to the previous track, if any.</summary>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        if (_index > 0)
        {
            _index--;
            await LoadAndPlayCurrentAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Adjusts the volume by <paramref name="delta"/> (clamped to 0–100).</summary>
    /// <param name="delta">The amount to add to the volume.</param>
    public void AdjustVolume(int delta)
    {
        _player.Volume = Math.Clamp(_player.Volume + delta, 0, 100);
        Changed?.Invoke();
    }

    /// <summary>Stops playback.</summary>
    public void Stop()
    {
        _player.Stop();
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _player.StateChanged -= OnPlayerStateChanged;
        await _player.DisposeAsync().ConfigureAwait(false);
    }

    private async Task LoadAndPlayCurrentAsync(CancellationToken cancellationToken)
    {
        if (Current is not { } item)
        {
            return;
        }

        await _player.LoadAsync(item, cancellationToken).ConfigureAwait(false);
        _player.Play();
        Changed?.Invoke();
    }

    private void OnPlayerStateChanged(object? sender, PlaybackState state)
    {
        if (state == PlaybackState.Ended)
        {
            // Auto-advance to the next track. Fire-and-forget is fine for a console app.
            _ = NextAsync();
        }

        Changed?.Invoke();
    }
}
