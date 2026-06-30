namespace YandexMusic.Player.Playback;

/// <summary>
/// The low-level audio sink. This is the seam that makes playback pluggable: a no-op/simulated
/// implementation drives the UI without any native dependency, while a real implementation (NAudio,
/// and later LibVLC for cross-platform) produces sound. Nothing above this interface knows which one
/// is in use.
/// </summary>
public interface IAudioPlayer : IAsyncDisposable
{
    /// <summary>The current playback state.</summary>
    PlaybackState State { get; }

    /// <summary>The current playback position.</summary>
    TimeSpan Position { get; }

    /// <summary>The duration of the loaded item.</summary>
    TimeSpan Duration { get; }

    /// <summary>The output volume, from 0 to 100.</summary>
    int Volume { get; set; }

    /// <summary>Whether this player can produce real audio in the current environment.</summary>
    bool ProducesSound { get; }

    /// <summary>Raised whenever <see cref="State"/> changes (for example a track ends).</summary>
    event EventHandler<PlaybackState>? StateChanged;

    /// <summary>Loads an item, resolving and opening its audio if available. Does not start playback.</summary>
    /// <param name="item">The item to load.</param>
    /// <param name="cancellationToken">A token to cancel loading.</param>
    Task LoadAsync(PlaybackItem item, CancellationToken cancellationToken = default);

    /// <summary>Starts (or restarts) playback of the loaded item.</summary>
    void Play();

    /// <summary>Pauses playback.</summary>
    void Pause();

    /// <summary>Resumes playback after a pause.</summary>
    void Resume();

    /// <summary>Stops playback and resets the position.</summary>
    void Stop();

    /// <summary>Seeks to an absolute position within the loaded item.</summary>
    /// <param name="position">The position to seek to.</param>
    void Seek(TimeSpan position);
}
