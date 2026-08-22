using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace YandexMusicTerminal.Diagnostics;

/// <summary>The outcome of the most recent update check.</summary>
public enum UpdateStatus
{
    /// <summary>No check has finished yet.</summary>
    Unknown,

    /// <summary>This build is the newest released one.</summary>
    UpToDate,

    /// <summary>A newer release exists; it is described by <see cref="UpdateChecker.Available"/>.</summary>
    UpdateAvailable,

    /// <summary>GitHub could not be reached, or its answer could not be read.</summary>
    Failed,
}

/// <summary>
/// Asks GitHub whether a newer release exists: once at startup and every half hour after that, so a
/// release published while the player is open is noticed without restarting it. Everything about it
/// is deliberately unobtrusive: it runs detached from startup, it never blocks the player, and every
/// failure is swallowed (an update check is not worth an error message). Two checks an hour are
/// nowhere near GitHub's unauthenticated rate limit.
///
/// <c>YM_PLAYER_NO_UPDATE_CHECK</c> turns the automatic schedule off. It deliberately does not
/// disable <see cref="CheckNowAsync"/>: opting out of being told means no nagging, not that an
/// explicit request from the "About" screen should do nothing.
/// </summary>
public sealed class UpdateChecker : IDisposable
{
    private const string ReleasesUrl = "https://api.github.com/repos/jrfrigat/YandexMusic/releases/latest";
    private const string OptOutVariable = "YM_PLAYER_NO_UPDATE_CHECK";
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);

    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>The newer release found, or <see langword="null"/> while none is known.</summary>
    public UpdateInfo? Available { get; private set; }

    /// <summary>What the most recent check concluded.</summary>
    public UpdateStatus Status { get; private set; } = UpdateStatus.Unknown;

    /// <summary>Whether a check is in flight, so a view can say so instead of looking stuck.</summary>
    public bool IsChecking { get; private set; }

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
    /// Starts the background schedule: a check right away, then one every half hour until
    /// <paramref name="cancellationToken"/> is cancelled. Returns immediately.
    /// </summary>
    /// <param name="cancellationToken">A token that stops the schedule.</param>
    public void Start(CancellationToken cancellationToken = default)
    {
        // Older builds throttled themselves to one check a day through this stamp file. The schedule
        // replaced it; clean the leftover up rather than leaving a dead file in the data directory.
        try
        {
            File.Delete(AppPaths.File("update-check"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A file we no longer read; failing to delete it changes nothing.
        }

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(OptOutVariable)))
        {
            return;
        }

        _ = Task.Run(() => LoopAsync(cancellationToken), CancellationToken.None);
    }

    /// <summary>
    /// Runs one check and reports what it concluded. Never throws and never blocks longer than the
    /// request timeout, so a view can call it directly from a key press.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the check.</param>
    /// <returns>The status after the check.</returns>
    public async Task<UpdateStatus> CheckNowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return Status;
        }

        IsChecking = true;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ymt", CurrentVersion));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var json = await http.GetStringAsync(ReleasesUrl, cancellationToken).ConfigureAwait(false);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("tag_name", out var tag) ||
                tag.GetString() is not { Length: > 0 } tagName)
            {
                return Conclude(UpdateStatus.Failed);
            }

            var latest = tagName.TrimStart('v', 'V');
            if (!IsNewer(latest, CurrentVersion))
            {
                Available = null;
                return Conclude(UpdateStatus.UpToDate);
            }

            var url = document.RootElement.TryGetProperty("html_url", out var link)
                ? link.GetString() ?? string.Empty
                : string.Empty;
            Available = new UpdateInfo(latest, url);
            return Conclude(UpdateStatus.UpdateAvailable);
        }
        catch (Exception)
        {
            // Best-effort by definition: no network, a rate limit, a changed payload — none of it is
            // the user's problem. A finding from an earlier check outlives a later failure, because
            // the release it points at has not stopped existing.
            return Conclude(Available is null ? UpdateStatus.Failed : UpdateStatus.UpdateAvailable);
        }
        finally
        {
            IsChecking = false;
            _ = _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose() => _gate.Dispose();

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

    private UpdateStatus Conclude(UpdateStatus status)
    {
        Status = status;
        return status;
    }

    private async Task LoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _ = await CheckNowAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await Task.Delay(CheckInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}

/// <summary>A release newer than the running build.</summary>
/// <param name="Version">The new version, without the tag's "v" prefix.</param>
/// <param name="Url">The release page.</param>
public sealed record UpdateInfo(string Version, string Url);
