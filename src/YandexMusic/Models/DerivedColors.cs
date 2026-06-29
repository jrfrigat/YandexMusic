namespace YandexMusic.Models;

/// <summary>The accent colours Yandex derives from a cover image (hex strings).</summary>
public sealed class DerivedColors
{
    /// <summary>The average colour.</summary>
    public string? Average { get; init; }

    /// <summary>The colour used behind waveform text.</summary>
    public string? WaveText { get; init; }

    /// <summary>The colour used by the mini player.</summary>
    public string? MiniPlayer { get; init; }

    /// <summary>The accent colour.</summary>
    public string? Accent { get; init; }
}
