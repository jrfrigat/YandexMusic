using System.Text.Json.Serialization;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>
/// One item of a combined radio session. The wire form keeps the payload in a single <c>data</c>
/// member whose shape follows <c>type</c>, so it is read into the typed property matching that type:
/// <c>TRACK</c> fills <see cref="Track"/> and <c>CLIP</c> fills <see cref="Clip"/>.
/// </summary>
[JsonConverter(typeof(CombinedSessionItemConverter))]
public sealed class CombinedSessionItem
{
    /// <summary>The item type. Known values: <c>CLIP</c> and <c>TRACK</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The track, when the type is <c>TRACK</c>.</summary>
    public Track? Track { get; init; }

    /// <summary>The clip, when the type is <c>CLIP</c>.</summary>
    public Clip? Clip { get; init; }
}
