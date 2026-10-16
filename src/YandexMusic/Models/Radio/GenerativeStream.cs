using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>
/// A generative station and its audio stream. Generative stations (<c>generative:focus</c>,
/// <c>generative:energy</c>, <c>generative:calm</c>, <c>generative:relax</c>) are endless
/// machine-produced audio rather than a chain of catalogue tracks, so they are played from a single
/// stream URL and report playback through their own feedback types.
/// </summary>
public sealed class GenerativeStream
{
    /// <summary>The station presentation: titles, colors and artwork.</summary>
    public GenerativeStreamData? Data { get; init; }

    /// <summary>The version of the generative model behind the station.</summary>
    public string? Version { get; init; }

    /// <summary>The stream to play, with the identifier feedback events refer to.</summary>
    public GenerativeStreamInfo? Stream { get; init; }
}

/// <summary>The presentation of a generative station: its titles, colors and artwork.</summary>
public sealed class GenerativeStreamData
{
    /// <summary>The station title.</summary>
    public string? Title { get; init; }

    /// <summary>The station subtitle.</summary>
    public string? Subtitle { get; init; }

    /// <summary>The background color, in HEX.</summary>
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; init; }

    /// <summary>The colors derived from the artwork.</summary>
    [JsonPropertyName("derivedColors")]
    public DerivedColors? DerivedColors { get; init; }

    /// <summary>The artwork URL.</summary>
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; init; }

    /// <summary>The video cover URI.</summary>
    [JsonPropertyName("videoCoverUri")]
    public string? VideoCoverUri { get; init; }

    /// <summary>The station description shown to the listener.</summary>
    public string? Explanations { get; init; }
}

/// <summary>The audio stream of a generative station.</summary>
public sealed class GenerativeStreamInfo
{
    /// <summary>
    /// The stream identifier. It must be echoed back as the <c>streamId</c> of every feedback event
    /// for the station, otherwise the server does not associate the event with this stream.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>The stream URL to play.</summary>
    public string? Url { get; init; }
}

/// <summary>The server's answer to a generative-station feedback event.</summary>
public sealed class GenerativeStreamFeedback
{
    /// <summary>
    /// Whether the client should fetch the stream again. The server sets this when the stream it
    /// handed out is no longer the one to play.
    /// </summary>
    [JsonPropertyName("reloadStream")]
    public bool? ReloadStream { get; init; }

    /// <summary>Whether the server considers the stream to be playing rather than paused.</summary>
    [JsonPropertyName("notPaused")]
    public bool? NotPaused { get; init; }
}
