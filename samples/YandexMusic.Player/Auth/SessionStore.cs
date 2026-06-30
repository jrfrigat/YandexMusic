using System.Text.Json;
using YandexMusic.Authentication;

namespace YandexMusic.Player.Auth;

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
/// Stores the session as JSON under the user's application-data folder. A real app would protect the
/// token (DPAPI/keychain); for a sample, a private file under the profile is enough.
/// </summary>
public sealed class FileSessionStore : ISessionStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _path;

    /// <summary>Creates a store at the default per-user location.</summary>
    public FileSessionStore()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "yandexmusic-player");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, "session.json");
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
        File.WriteAllText(_path, JsonSerializer.Serialize(snapshot, SerializerOptions));
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
