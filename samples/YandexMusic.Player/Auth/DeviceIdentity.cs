using System.Globalization;

namespace YandexMusic.Player.Auth;

/// <summary>
/// This installation's identity in the account's device list. Ynison remembers every device id that
/// ever registered and keeps showing it — offline — in every client of the account, so an id that
/// changes per run leaves a trail of dead entries the user has to look at forever. The id is
/// therefore generated once and kept in the player's own data directory, next to the session.
/// </summary>
public static class DeviceIdentity
{
    /// <summary>Reads this installation's device id, creating it on first use.</summary>
    /// <returns>A stable identifier, or a fresh random one when the file cannot be used.</returns>
    public static string GetOrCreate()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "yandexmusic-player",
            "device-id");

        try
        {
            if (File.Exists(path))
            {
                var stored = File.ReadAllText(path).Trim();
                if (stored.Length > 0)
                {
                    return stored;
                }
            }

            var created = Create();
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, created);
            return created;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Without a file the id cannot be stable; a fresh one still works for this run.
            return Create();
        }
    }

    /// <summary>
    /// The name other clients of the account show for this device. It carries the machine name for
    /// the same reason the phone shows "OnePlus CPH2747": with several installations on one account,
    /// an identical name for all of them is useless.
    /// </summary>
    /// <returns>The display name.</returns>
    public static string DisplayName()
    {
        var machine = Environment.MachineName;
        return string.IsNullOrWhiteSpace(machine) ? "YandexMusic .NET" : $"YandexMusic .NET ({machine})";
    }

    private static string Create() => Guid.NewGuid().ToString("N")[..16].ToLower(CultureInfo.InvariantCulture);
}
