using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;

namespace YandexMusic.Models.MusicHistory;

/// <summary>A single history entry: either a context or a track, with its identifier and resolved model.</summary>
public sealed class MusicHistoryItem
{
    /// <summary>The entry type. Known values: <c>track</c>, <c>album</c>, <c>artist</c>, <c>playlist</c>, <c>wave</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The entry payload: its identifier and, when requested, the resolved full model.</summary>
    public MusicHistoryItemData? Data { get; init; }
}

/// <summary>The payload of a <see cref="MusicHistoryItem"/>: an identifier plus an optional resolved model.</summary>
public sealed class MusicHistoryItemData
{
    /// <summary>The identifier of the referenced entity.</summary>
    [JsonPropertyName("item_id")]
    public MusicHistoryItemId? ItemId { get; init; }

    /// <summary>The resolved full model, when the request asked for full models.</summary>
    [JsonPropertyName("full_model")]
    public MusicHistoryFullModel? FullModel { get; init; }
}

/// <summary>The identifier of a history entry. Only the fields relevant to its type are populated.</summary>
public sealed class MusicHistoryItemId
{
    /// <summary>The album or artist identifier.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; init; }

    /// <summary>The track identifier (for <c>track</c> entries).</summary>
    [JsonPropertyName("track_id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? TrackId { get; init; }

    /// <summary>The album the track is referenced from (for <c>track</c> entries).</summary>
    [JsonPropertyName("album_id")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? AlbumId { get; init; }

    /// <summary>The playlist owner identifier (for <c>playlist</c> entries).</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Uid { get; init; }

    /// <summary>The playlist kind (for <c>playlist</c> entries).</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Kind { get; init; }

    /// <summary>The wave seeds (for <c>wave</c> entries).</summary>
    public IReadOnlyList<string>? Seeds { get; init; }
}

/// <summary>
/// The resolved full model of a history entry. For <c>track</c> entries the <see cref="Track"/> branch is
/// populated; for every other type the <see cref="Context"/> branch carries the album, artist, playlist or wave.
/// </summary>
public sealed class MusicHistoryFullModel
{
    /// <summary>The resolved track, for <c>track</c> entries.</summary>
    public Track? Track { get; init; }

    /// <summary>The resolved context (album, artist, playlist or wave), for non-track entries.</summary>
    public MusicHistoryContextFullModel? Context { get; init; }
}

/// <summary>The resolved context of a non-track history entry.</summary>
public sealed class MusicHistoryContextFullModel
{
    /// <summary>The album, when the entry is an album context.</summary>
    public Album? Album { get; init; }

    /// <summary>The artist, when the entry is an artist context.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The playlist, when the entry is a playlist context.</summary>
    public Playlist? Playlist { get; init; }

    /// <summary>The wave, when the entry is a wave context.</summary>
    public Wave? Wave { get; init; }

    /// <summary>The artists credited on the album, for an album context.</summary>
    public IReadOnlyList<Artist>? Artists { get; init; }

    /// <summary>Whether the context is currently available.</summary>
    public bool? Available { get; init; }

    /// <summary>The number of tracks in the context.</summary>
    [JsonPropertyName("tracks_count")]
    public int? TracksCount { get; init; }

    /// <summary>The foreground image URL of a simple wave context.</summary>
    [JsonPropertyName("simple_wave_foreground_image_url")]
    public string? SimpleWaveForegroundImageUrl { get; init; }

    /// <summary>The background color of a simple wave context.</summary>
    [JsonPropertyName("simple_wave_background_color")]
    public string? SimpleWaveBackgroundColor { get; init; }
}

/// <summary>The response of resolving history item identifiers into full models.</summary>
public sealed class MusicHistoryItems
{
    /// <summary>The resolved history items.</summary>
    public IReadOnlyList<MusicHistoryItem>? Items { get; init; }
}
