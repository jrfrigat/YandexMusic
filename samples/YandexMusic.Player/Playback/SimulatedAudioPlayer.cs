using System.Diagnostics;

namespace YandexMusic.Player.Playback;

/// <summary>
/// An <see cref="IAudioPlayer"/> that models a playing track without producing any sound. It advances
/// a position clock in real time so the whole UI — progress, timers, "track ended" transitions and
/// volume — works identically to a real player. This keeps the app fully functional without a
/// subscription, a token, or any native audio dependency, and is the default everywhere.
/// </summary>
public sealed class SimulatedAudioPlayer : IAudioPlayer
{
    private readonly Stopwatch _stopwatch = new();
    private readonly Lock _gate = new();
    private Timer? _ticker;
    private TimeSpan _basePosition;
    private TimeSpan _duration;
    private PlaybackState _state = PlaybackState.Idle;
    private int _volume = 60;

    /// <inheritdoc />
    public PlaybackState State
    {
        get { lock (_gate) { return _state; } }
    }

    /// <inheritdoc />
    public TimeSpan Position
    {
        get
        {
            lock (_gate)
            {
                var elapsed = _state == PlaybackState.Playing ? _stopwatch.Elapsed : TimeSpan.Zero;
                var position = _basePosition + elapsed;
                return position > _duration ? _duration : position;
            }
        }
    }

    /// <inheritdoc />
    public TimeSpan Duration
    {
        get { lock (_gate) { return _duration; } }
    }

    /// <inheritdoc />
    public int Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0, 100);
    }

    /// <inheritdoc />
    public bool ProducesSound => false;

    /// <inheritdoc />
    public event EventHandler<PlaybackState>? StateChanged;

    /// <inheritdoc />
    public Task LoadAsync(PlaybackItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_gate)
        {
            _stopwatch.Reset();
            _basePosition = TimeSpan.Zero;
            _duration = item.Duration > TimeSpan.Zero ? item.Duration : TimeSpan.FromMinutes(3);
        }

        SetState(PlaybackState.Stopped);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Play()
    {
        lock (_gate)
        {
            _stopwatch.Restart();
            _basePosition = TimeSpan.Zero;
        }

        SetState(PlaybackState.Playing);
        StartTicker();
    }

    /// <inheritdoc />
    public void Pause()
    {
        lock (_gate)
        {
            if (_state != PlaybackState.Playing)
            {
                return;
            }

            _basePosition += _stopwatch.Elapsed;
            _stopwatch.Reset();
        }

        SetState(PlaybackState.Paused);
    }

    /// <inheritdoc />
    public void Resume()
    {
        lock (_gate)
        {
            if (_state != PlaybackState.Paused)
            {
                return;
            }

            _stopwatch.Restart();
        }

        SetState(PlaybackState.Playing);
        StartTicker();
    }

    /// <inheritdoc />
    public void Stop()
    {
        lock (_gate)
        {
            _stopwatch.Reset();
            _basePosition = TimeSpan.Zero;
        }

        SetState(PlaybackState.Stopped);
    }

    /// <inheritdoc />
    public void Seek(TimeSpan position)
    {
        lock (_gate)
        {
            _basePosition = TimeSpan.FromTicks(Math.Clamp(position.Ticks, 0, _duration.Ticks));
            if (_state == PlaybackState.Playing)
            {
                _stopwatch.Restart();
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _ticker?.Dispose();
        return ValueTask.CompletedTask;
    }

    private void StartTicker()
    {
        _ticker ??= new Timer(_ => CheckForEnd(), state: null, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
    }

    private void CheckForEnd()
    {
        if (State == PlaybackState.Playing && Position >= Duration && Duration > TimeSpan.Zero)
        {
            SetState(PlaybackState.Ended);
        }
    }

    private void SetState(PlaybackState state)
    {
        lock (_gate)
        {
            if (_state == state)
            {
                return;
            }

            _state = state;
        }

        StateChanged?.Invoke(this, state);
    }
}
