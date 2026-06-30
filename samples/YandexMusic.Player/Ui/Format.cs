namespace YandexMusic.Player.Ui;

/// <summary>Small formatting helpers for the terminal UI.</summary>
public static class Format
{
    /// <summary>Formats a duration as <c>m:ss</c> (or <c>h:mm:ss</c> when an hour or longer).</summary>
    /// <param name="span">The duration.</param>
    /// <returns>The formatted duration.</returns>
    public static string Duration(TimeSpan span)
        => span >= TimeSpan.FromHours(1)
            ? $"{(int)span.TotalHours}:{span.Minutes:D2}:{span.Seconds:D2}"
            : $"{(int)span.TotalMinutes}:{span.Seconds:D2}";

    /// <summary>Truncates <paramref name="text"/> to <paramref name="max"/> characters with an ellipsis.</summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="max">The maximum length.</param>
    /// <returns>The truncated text.</returns>
    public static string Truncate(string text, int max)
        => string.IsNullOrEmpty(text) || text.Length <= max ? text : text[..Math.Max(0, max - 1)] + "…";
}
