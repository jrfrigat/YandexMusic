using System.Text.Json.Serialization;

namespace YandexMusic.Models.MusicHistory;

/// <summary>The user's listening history, grouped into day tabs.</summary>
public sealed class MusicHistory
{
    /// <summary>The day tabs the history is grouped into, newest first.</summary>
    [JsonPropertyName("history_tabs")]
    public IReadOnlyList<MusicHistoryTab>? HistoryTabs { get; init; }
}

/// <summary>A single day tab of the listening history.</summary>
public sealed class MusicHistoryTab
{
    /// <summary>The tab date in <c>YYYY-MM-DD</c> form.</summary>
    public string? Date { get; init; }

    /// <summary>The listening groups recorded on this day.</summary>
    public IReadOnlyList<MusicHistoryGroup>? Items { get; init; }
}

/// <summary>A group of tracks listened to within a single context (album, artist, playlist or wave).</summary>
public sealed class MusicHistoryGroup
{
    /// <summary>The context the tracks were played from.</summary>
    public MusicHistoryItem? Context { get; init; }

    /// <summary>The tracks played within the context.</summary>
    public IReadOnlyList<MusicHistoryItem>? Tracks { get; init; }
}
