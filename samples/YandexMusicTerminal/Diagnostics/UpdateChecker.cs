using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace YandexMusicTerminal.Diagnostics;

/// <summary>
/// Asks GitHub once a day whether a newer release exists and, if so, offers the news to the main
/// menu. Everything about it is deliberately unobtrusive: it runs detached from startup, it never
/// blocks or slows the player down, every failure is swallowed (an update check is not worth an
/// error message), it caches its answer so the unauthenticated rate limit is never a concern, and
/// setting <c>YM_PLAYER_NO_UPDATE_CHECK</c> turns it off entirely.
/// </summary>
public sealed class UpdateChecker
{
    private const string ReleasesUrl = "https://api.github.com/repos/jrfrigat/YandexMusic/releases/latest";
    private const string OptOutVariable = "YM_PLAYER_NO_UPDATE_CHECK";
    private static readonly TimeSpan CheckInterval = TimeSpan.FromDays(1);

    private readonly string _statePath;

    /// <summary>Creates the checker.</summary>
    public UpdateChecker()
    {
        _statePath = AppPaths.File("update-check");
    }

    /// <summary>The newer release found, or <see langword="null"/> while none is known.</summary>
    public UpdateInfo? Available { get; private set; }

    /// <summary>The version this build reports, without any build metadata.</summary>
    public static string CurrentVersion
    {
        get
        {
            var informational = typeof(UpdateChecker).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var version = informational ?? typeof(UpdateChecker).Assembly.GetName().Version?.ToString() ?? "0.0.0";
            var plus = version.IndexOf('+', StringComparison.Ordinal);
            return plus < 0 ? version : version[..plus];
        }
    }

    /// <summary>
    /// Starts the check in the background. Returns immediately; <see cref="Available"/> is filled in
    /// later, if at all.
    /// </summary>
    public void StartCheck()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(OptOutVariable)))
        {
            return;
        }

        _ = Task.Run(CheckAsync);
    }

    private async Task CheckAsync()
    {
        try
        {
            if (!DueForCheck())
            {
                return;
            }

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ymt", CurrentVersion));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var json = await http.GetStringAsync(ReleasesUrl).ConfigureAwait(false);
            RecordCheck();

            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("tag_name", out var tag) ||
                tag.GetString() is not { Length: > 0 } tagName)
            {
                return;
            }

            var latest = tagName.TrimStart('v', 'V');
            if (IsNewer(latest, CurrentVersion))
            {
                var url = document.RootElement.TryGetProperty("html_url", out var link)
                    ? link.GetString() ?? string.Empty
                    : string.Empty;
                Available = new UpdateInfo(latest, url);
            }
        }
        catch (Exception)
        {
            // An update check is best-effort by definition: no network, a rate limit, a changed
            // payload — none of it is the user's problem, and none of it should be shown.
        }
    }

    /// <summary>
    /// Compares two release versions, ignoring anything after a pre-release dash. Returns false when
    /// either side is unparseable, so an odd version never produces a bogus "update available".
    /// </summary>
    internal static bool IsNewer(string candidate, string current)
        => TryParse(candidate, out var newer) && TryParse(current, out var running) && newer > running;

    private static bool TryParse(string value, out Version version)
    {
        var dash = value.IndexOf('-', StringComparison.Ordinal);
        var core = dash < 0 ? value : value[..dash];
        return Version.TryParse(core, out version!);
    }

    private bool DueForCheck()
    {
        try
        {
            if (!File.Exists(_statePath))
            {
                return true;
            }

            var stamp = File.ReadAllText(_statePath).Trim();
            return !DateTime.TryParse(stamp, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var last)
                || DateTime.UtcNow - last > CheckInterval;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return true;
        }
    }

    private void RecordCheck()
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
            File.WriteAllText(_statePath, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Without the stamp the check simply runs again next time.
        }
    }
}

/// <summary>A release newer than the running build.</summary>
/// <param name="Version">The new version, without the tag's "v" prefix.</param>
/// <param name="Url">The release page.</param>
public sealed record UpdateInfo(string Version, string Url);
