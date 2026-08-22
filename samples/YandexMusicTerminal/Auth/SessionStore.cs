using System.Text.Json;
using YandexMusic.Authentication;

namespace YandexMusicTerminal.Auth;

/// <summary>Persists the authenticated session between runs so the user does not sign in every time.</summary>
public interface ISessionStore
{
    /// <summary>Loads the saved session, or <see langword="null"/> when none exists.</summary>
    AuthSnapshot? Load();

    /// <summary>Saves the session.</summary>
    /// <param name="snapshot">The session snapshot to persist.</param>
    void Save(AuthSnapshot snapshot);

    /// <summary>Deletes the saved session.</summary>
    void Clear();
}

/// <summary>
/// Stores the session as JSON under the user's application-data folder
/// (<c>%APPDATA%\ymt\session.json</c> on Windows, <c>~/.config/ymt/session.json</c>
/// elsewhere). The file is private to the user
/// (0600 on Unix) and written atomically, but its contents are plain JSON — a real app would use
/// DPAPI/keychain instead.
/// </summary>
public sealed class FileSessionStore : ISessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _path;

    /// <summary>Creates a store at the default per-user location.</summary>
    public FileSessionStore()
    {
        _path = AppPaths.File("session.json");
    }

    /// <inheritdoc />
    public AuthSnapshot? Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AuthSnapshot>(File.ReadAllText(_path), SerializerOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public void Save(AuthSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        // Write to a temp file and rename, so a crash mid-write cannot truncate the session.
        var tempPath = _path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(snapshot, SerializerOptions));
        if (!OperatingSystem.IsWindows())
        {
            // The token and session cookies are account access; keep the file to the owner only.
            File.SetUnixFileMode(tempPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }

        File.Move(tempPath, _path, overwrite: true);
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }
}
