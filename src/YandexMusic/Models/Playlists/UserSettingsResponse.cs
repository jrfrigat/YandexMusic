using YandexMusic.Models.Account;

namespace YandexMusic.Models.Playlists;

/// <summary>The envelope wrapping a user's settings as returned by the settings endpoint.</summary>
public sealed class UserSettingsResponse
{
    /// <summary>The user's playback and privacy preferences.</summary>
    public UserSettings? UserSettings { get; init; }
}
