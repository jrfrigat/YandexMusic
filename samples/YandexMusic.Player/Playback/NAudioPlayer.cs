#if WINDOWS
using System.Runtime.Versioning;
using NAudio.Wave;

namespace YandexMusic.Player.Playback;

/// <summary>
/// A real audio player backed by NAudio's Media Foundation reader and the WaveOut device. It streams
/// the track's direct MP3 URL and offers true volume control. Windows-only; on other platforms (or
/// when no stream URL is available) the app falls back to the simulated player. Swapping NAudio for a
/// cross-platform backend (e.g. LibVLC) is a matter of providing another <see cref="IAudioPlayer"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class NAudioPlayer : IAudioPlayer
{
    private readonly Lock _gate = new();
    private WaveOut? _output;
    private MediaFoundationReader? _reader;
    private PlaybackState _state = PlaybackState.Idle;
    private int _volume = 60;
    private bool _stopRequested;

    /// <inheritdoc />
    public PlaybackState State
    {
        get { lock (_gate) { return _state; } }
    }

    /// <inheritdoc />
    public TimeSpan Position
    {
        get { lock (_gate) { return _reader?.CurrentTime ?? TimeSpan.Zero; } }
    }

    /// <inheritdoc />
    public TimeSpan Duration
    {
        get { lock (_gate) { return _reader?.TotalTime ?? TimeSpan.Zero; } }
    }

    /// <inheritdoc />
    public int Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0, 100);
            lock (_gate)
            {
                if (_output is not null)
                {
                    _output.Volume = _volume / 100f;
                }
            }
        }
    }

    /// <inheritdoc />
    public bool ProducesSound => true;

    /// <inheritdoc />
    public event EventHandler<PlaybackState>? StateChanged;

    /// <inheritdoc />
    public async Task LoadAsync(PlaybackItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        SetState(PlaybackState.Buffering);

        var url = await item.ResolveStreamUrlAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(url))
        {
            SetState(PlaybackState.Error);
            throw new InvalidOperationException("No stream URL is available for this track.");
        }

        lock (_gate)
        {
            DisposeBackend();
            _stopRequested = false;
            _reader = new MediaFoundationReader(url);
            _output = new WaveOut { Volume = _volume / 100f };
            _output.Init(_reader);
            _output.PlaybackStopped += OnPlaybackStopped;
        }

        SetState(PlaybackState.Stopped);
    }

    /// <inheritdoc />
    public void Play()
    {
        lock (_gate)
        {
            if (_reader is not null)
            {
                _reader.CurrentTime = TimeSpan.Zero;
            }

            _output?.Play();
        }

        SetState(PlaybackState.Playing);
    }

    /// <inheritdoc />
    public void Pause()
    {
        lock (_gate)
        {
            _output?.Pause();
        }

        SetState(PlaybackState.Paused);
    }

    /// <inheritdoc />
    public void Resume()
    {
        lock (_gate)
        {
            _output?.Play();
        }

        SetState(PlaybackState.Playing);
    }

    /// <inheritdoc />
    public void Stop()
    {
        lock (_gate)
        {
            _stopRequested = true;
            _output?.Stop();
            if (_reader is not null)
            {
                _reader.CurrentTime = TimeSpan.Zero;
            }
        }

        SetState(PlaybackState.Stopped);
    }

    /// <inheritdoc />
    public void Seek(TimeSpan position)
    {
        lock (_gate)
        {
            if (_reader is not null)
            {
                _reader.CurrentTime = TimeSpan.FromTicks(Math.Clamp(position.Ticks, 0, _reader.TotalTime.Ticks));
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_gate)
        {
            DisposeBackend();
        }

        return ValueTask.CompletedTask;
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        // PlaybackStopped fires both for an explicit Stop() and for a natural end; only the latter
        // should advance the queue.
        if (!_stopRequested)
        {
            SetState(PlaybackState.Ended);
        }
    }

    private void DisposeBackend()
    {
        if (_output is not null)
        {
            _output.PlaybackStopped -= OnPlaybackStopped;
            _output.Dispose();
            _output = null;
        }

        _reader?.Dispose();
        _reader = null;
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
#endif
