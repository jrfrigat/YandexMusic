namespace YandexMusic.Models.Queue;

/// <summary>Describes the content a queue was built from, such as a playlist, album or radio station.</summary>
public sealed class Context
{
    /// <summary>The content type, for example <c>various</c>, <c>my_music</c>, <c>radio</c>, <c>playlist</c> or <c>artist</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The identifier of the content, when it has one (absent for <c>my_music</c> or <c>various</c>).</summary>
    public string? Id { get; init; }

    /// <summary>A human-readable description of the content, such as a playlist or station name.</summary>
    public string? Description { get; init; }
}
