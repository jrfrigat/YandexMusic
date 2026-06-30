namespace YandexMusic.Player.Playback;

/// <summary>
/// An <see cref="IAudioPlayer"/> that prefers a real backend but transparently falls back to the
/// simulated one per track. If the real player cannot load an item (no platform support, no stream
/// URL, or an open error) the simulated player takes over so the UI never breaks. This keeps the rest
/// of the app oblivious to whether sound is actually coming out.
/// </summary>
public sealed class ResilientAudioPlayer : IAudioPlayer
{
    private readonly IAudioPlayer? _primary;
    private readonly IAudioPlayer _fallback;
    private IAudioPlayer _active;

    /// <summary>Creates a resilient player over an optional real backend and a required fallback.</summary>
    /// <param name="primary">The preferred (real) backend, or <see langword="null"/> if unavailable.</param>
    /// <param name="fallback">The always-available fallback (typically the simulated player).</param>
    public ResilientAudioPlayer(IAudioPlayer? primary, IAudioPlayer fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        _primary = primary;
        _fallback = fallback;
        _active = fallback;

        if (_primary is not null)
        {
            _primary.StateChanged += ReRaise;
        }

        _fallback.StateChanged += ReRaise;
    }

    /// <inheritdoc />
    public PlaybackState State => _active.State;

    /// <inheritdoc />
    public TimeSpan Position => _active.Position;

    /// <inheritdoc />
    public TimeSpan Duration => _active.Duration;

    /// <inheritdoc />
    public int Volume
    {
        get => _active.Volume;
        set
        {
            if (_primary is not null)
            {
                _primary.Volume = value;
            }

            _fallback.Volume = value;
        }
    }

    /// <inheritdoc />
    public bool ProducesSound => _active.ProducesSound;

    /// <inheritdoc />
    public event EventHandler<PlaybackState>? StateChanged;

    /// <inheritdoc />
    public async Task LoadAsync(PlaybackItem item, CancellationToken cancellationToken = default)
    {
        if (_primary is not null)
        {
            try
            {
                await _primary.LoadAsync(item, cancellationToken).ConfigureAwait(false);
                _active = _primary;
                return;
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested == false)
            {
                // Fall back to the simulated player below.
            }
        }

        _active = _fallback;
        await _fallback.LoadAsync(item, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Play() => _active.Play();

    /// <inheritdoc />
    public void Pause() => _active.Pause();

    /// <inheritdoc />
    public void Resume() => _active.Resume();

    /// <inheritdoc />
    public void Stop() => _active.Stop();

    /// <inheritdoc />
    public void Seek(TimeSpan position) => _active.Seek(position);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _fallback.StateChanged -= ReRaise;
        await _fallback.DisposeAsync().ConfigureAwait(false);

        if (_primary is not null)
        {
            _primary.StateChanged -= ReRaise;
            await _primary.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void ReRaise(object? sender, PlaybackState state)
    {
        if (ReferenceEquals(sender, _active))
        {
            StateChanged?.Invoke(this, state);
        }
    }
}
