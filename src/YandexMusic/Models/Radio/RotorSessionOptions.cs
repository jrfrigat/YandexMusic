namespace YandexMusic.Models.Radio;

/// <summary>
/// Optional parameters for opening or cloning a radio session. Every member left
/// <see langword="null"/> is omitted from the request, so the server default applies.
/// </summary>
public sealed record RotorSessionOptions
{
    /// <summary>Tracks already played (as <c>id</c> or <c>id:albumId</c>), so the session does not serve them again.</summary>
    public IReadOnlyList<string>? Queue { get; init; }

    /// <summary>The track to start the session from, as <c>id</c> or <c>id:albumId</c>.</summary>
    public string? TrackToStartFrom { get; init; }

    /// <summary>Whether the response should include the first batch of tracks. The server default is yes.</summary>
    public bool? IncludeTracksInResponse { get; init; }

    /// <summary>Whether the response should include the wave model (<see cref="RotorSession.Wave"/>).</summary>
    public bool? IncludeWaveModel { get; init; }

    /// <summary>Whether the session is interactive.</summary>
    public bool? Interactive { get; init; }

    /// <summary>Whether playback should be kept out of the recommendations.</summary>
    public bool? Incognito { get; init; }

    /// <summary>Whether the child mode is on.</summary>
    public bool? Child { get; init; }

    /// <summary>Whether explicit content is allowed.</summary>
    public bool? AllowExplicit { get; init; }
}
