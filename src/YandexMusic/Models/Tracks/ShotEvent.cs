namespace YandexMusic.Models.Tracks;

/// <summary>A set of Alice "shots" (short voice intros) returned to play after a track.</summary>
public sealed class ShotEvent
{
    /// <summary>The event identifier.</summary>
    public string EventId { get; init; } = string.Empty;

    /// <summary>The shots to play.</summary>
    public IReadOnlyList<Shot>? Shots { get; init; }
}

/// <summary>A single Alice shot.</summary>
public sealed class Shot
{
    /// <summary>The play order of the shot.</summary>
    public int Order { get; init; }

    /// <summary>Whether the shot has already been played.</summary>
    public bool Played { get; init; }

    /// <summary>The shot payload, when present.</summary>
    public ShotData? ShotData { get; init; }

    /// <summary>The shot identifier.</summary>
    public string ShotId { get; init; } = string.Empty;

    /// <summary>The shot status. Known value: <c>ready</c>.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>The payload of an Alice shot: the cover, the audio and the spoken text.</summary>
public sealed class ShotData
{
    /// <summary>The cover image URI template.</summary>
    public string CoverUri { get; init; } = string.Empty;

    /// <summary>The audio URL of the shot.</summary>
    public string MdsUrl { get; init; } = string.Empty;

    /// <summary>The spoken text of the shot.</summary>
    public string ShotText { get; init; } = string.Empty;

    /// <summary>The shot type, when present.</summary>
    public ShotType? ShotType { get; init; }
}

/// <summary>The type of an Alice shot.</summary>
public sealed class ShotType
{
    /// <summary>The type identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The type display title.</summary>
    public string Title { get; init; } = string.Empty;
}

/// <summary>The envelope the after-track endpoint wraps its shot event in.</summary>
internal sealed class AfterTrackResponse
{
    /// <summary>The shot event, when present.</summary>
    public ShotEvent? ShotEvent { get; init; }
}
