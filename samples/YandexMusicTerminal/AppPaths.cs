namespace YandexMusicTerminal;

/// <summary>
/// Where the terminal keeps its own files: the saved session, the device identity, the request
/// journal and the update-check stamp. One place decides the directory, so a rename stays a
/// one-line change instead of four that can drift apart.
/// </summary>
public static class AppPaths
{
    private const string DirectoryName = "ymt";
    private const string LegacyDirectoryName = "yandexmusic-player";

    private static readonly Lazy<string> Root = new(Resolve, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// The terminal's data directory — <c>%APPDATA%\ymt</c> on Windows,
    /// <c>~/.config/ymt</c> elsewhere. It is created on first use.
    /// </summary>
    public static string DataDirectory => Root.Value;

    /// <summary>Builds a path to a file inside <see cref="DataDirectory"/>.</summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The full path.</returns>
    public static string File(string fileName) => Path.Combine(DataDirectory, fileName);

    private static string Resolve()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(appData, DirectoryName);

        try
        {
            // The app used to be called "yandexmusic-player". Carry the old directory over on first
            // run so the rename does not silently sign everyone out.
            var legacy = Path.Combine(appData, LegacyDirectoryName);
            if (!Directory.Exists(path) && Directory.Exists(legacy))
            {
                Directory.Move(legacy, path);
            }
            else
            {
                _ = Directory.CreateDirectory(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Whoever writes into the directory reports its own failure; resolving a path must not throw.
        }

        return path;
    }
}
