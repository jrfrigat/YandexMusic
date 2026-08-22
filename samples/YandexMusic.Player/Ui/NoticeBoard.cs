namespace YandexMusic.Player.Ui;

/// <summary>
/// The app-wide channel for a short message a screen leaves behind when it closes (an error, a
/// refusal to open). Writing such a message straight to the console would strand it: every screen
/// that follows is a live view rendered *below* the cursor, so the line stays in the scrollback for
/// the rest of the session. A posted notice is rendered by the main menu instead and expires on its
/// own, exactly like the in-panel toasts of the player and the remote.
/// </summary>
public sealed class NoticeBoard
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(4);

    private string _message = string.Empty;
    private DateTime _postedAt;

    /// <summary>Posts a message to be shown on the next renders of the main menu.</summary>
    /// <param name="message">The plain (unescaped, markup-free) message text.</param>
    public void Post(string message)
    {
        _message = message ?? string.Empty;
        _postedAt = DateTime.UtcNow;
    }

    /// <summary>Reads the message still worth showing.</summary>
    /// <returns>The pending message, or <see langword="null"/> when there is none or it has expired.</returns>
    public string? Peek()
        => _message.Length > 0 && DateTime.UtcNow - _postedAt < Lifetime ? _message : null;
}
