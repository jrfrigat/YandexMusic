using System.Text.Json;
using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Landing;

/// <summary>The landing page payload: an ordered set of content blocks personalized for the user.</summary>
public sealed class Landing
{
    /// <summary>Whether the seasonal "pumpkin" theme is active.</summary>
    public bool Pumpkin { get; init; }

    /// <summary>The landing content identifier.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string ContentId { get; init; } = string.Empty;

    /// <summary>The content blocks, in display order.</summary>
    public IReadOnlyList<Block> Blocks { get; init; } = [];
}

/// <summary>A single landing block grouping a list of entities of one kind.</summary>
public sealed class Block
{
    /// <summary>The block identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The block type, for example <c>personal-playlists</c> or <c>play-contexts</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The type used when building "from" attribution strings.</summary>
    public string TypeForFrom { get; init; } = string.Empty;

    /// <summary>The block title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The entities contained in the block.</summary>
    public IReadOnlyList<BlockEntity> Entities { get; init; } = [];

    /// <summary>The block description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// The block-level payload, whose concrete shape depends on <see cref="Type"/> (for example
    /// <see cref="PersonalPlaylistsData"/> for <c>personal-playlists</c> or
    /// <see cref="PlayContextsData"/> for <c>play-contexts</c>). Exposed as a raw JSON element.
    /// </summary>
    public JsonElement? Data { get; init; }
}

/// <summary>A single entity inside a landing <see cref="Block"/>.</summary>
public sealed class BlockEntity
{
    /// <summary>The entity identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The entity type, for example <c>personal-playlist</c>, <c>promotion</c>, <c>album</c>,
    /// <c>playlist</c>, <c>chart-item</c>, <c>play-context</c>, or <c>mix-link</c>.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The entity payload, whose concrete shape depends on <see cref="Type"/> (for example a
    /// <see cref="GeneratedPlaylist"/>, <see cref="Promotion"/>, <see cref="ChartItem"/>,
    /// <see cref="PlayContext"/>, or <see cref="MixLink"/>). Exposed as a raw JSON element.
    /// </summary>
    public JsonElement? Data { get; init; }
}

/// <summary>The block-level payload for a <c>personal-playlists</c> block.</summary>
public sealed class PersonalPlaylistsData
{
    /// <summary>Whether the user has completed the onboarding wizard.</summary>
    public bool IsWizardPassed { get; init; }
}

/// <summary>The block-level payload for a <c>play-contexts</c> block.</summary>
public sealed class PlayContextsData
{
    /// <summary>Other recently played tracks associated with the contexts.</summary>
    public IReadOnlyList<TrackShortOld> OtherTracks { get; init; } = [];
}

/// <summary>A previously played play-context (a queue rooted at some catalogue object).</summary>
public sealed class PlayContext
{
    /// <summary>The client that produced the context.</summary>
    [JsonPropertyName("client")]
    public string Client { get; init; } = string.Empty;

    /// <summary>The context identifier (for example an album or playlist reference).</summary>
    public string Context { get; init; } = string.Empty;

    /// <summary>The specific item within the context.</summary>
    public string ContextItem { get; init; } = string.Empty;

    /// <summary>The tracks that made up the context.</summary>
    public IReadOnlyList<TrackShortOld> Tracks { get; init; } = [];
}

/// <summary>A legacy short track reference (a track id plus the time it was played).</summary>
public sealed class TrackShortOld
{
    /// <summary>The referenced track, when present.</summary>
    public TrackId? TrackId { get; init; }

    /// <summary>The play timestamp.</summary>
    public string Timestamp { get; init; } = string.Empty;
}
