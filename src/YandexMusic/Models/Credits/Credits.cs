using System.Text.Json.Serialization;

namespace YandexMusic.Models.Credits;

/// <summary>The production credits of a track or clip.</summary>
public sealed class Credits
{
    /// <summary>The individual credit entries.</summary>
    [JsonPropertyName("credits")]
    public IReadOnlyList<Credit>? Items { get; init; }
}

/// <summary>A single production credit (a role and the participant who filled it).</summary>
public sealed class Credit
{
    /// <summary>The role, for example <c>composer</c> or <c>label</c>.</summary>
    public string? Title { get; init; }

    /// <summary>The name of the participant.</summary>
    public string? Value { get; init; }
}
