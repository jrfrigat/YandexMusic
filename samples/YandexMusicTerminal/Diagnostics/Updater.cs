using System.Diagnostics;

namespace YandexMusicTerminal.Diagnostics;

/// <summary>What the caller has to do after asking for an update.</summary>
public enum UpdateLaunch
{
    /// <summary>The installer could not be started; nothing was changed.</summary>
    Failed,

    /// <summary>The installer is waiting for this process to end. Quit now, or it cannot finish.</summary>
    QuitToApply,

    /// <summary>The installer finished. This process is still the old build, so restart it.</summary>
    RestartToApply,
}

/// <summary>
/// Runs the same one-command installer the README advertises, so an available update is one key
/// press rather than a command to copy out of a message.
///
/// The two platforms need genuinely different handling, and the reason is file locking. On Windows
/// the running <c>ymt.exe</c> cannot be replaced while it runs, so the installer is started detached
/// in its own console with instructions to wait for this process to exit first, and the player quits
/// immediately. On Linux a running executable can be replaced underneath itself, so the installer
/// runs inline in the current terminal where its output is visible, and only then does the player
/// exit — the process in memory is still the old build either way.
/// </summary>
public static class Updater
{
    private const string WindowsScriptUrl = "https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.ps1";
    private const string LinuxScriptUrl = "https://raw.githubusercontent.com/jrfrigat/YandexMusic/main/scripts/install.sh";

    /// <summary>
    /// Whether this platform has a published build the installer can fetch. macOS has neither a
    /// release archive nor an installer, so there the update is only ever a manual affair.
    /// </summary>
    public static bool IsSupported => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

    /// <summary>The command a user would run by hand, shown when <see cref="IsSupported"/> is false or a launch fails.</summary>
    public static string Command => OperatingSystem.IsWindows()
        ? $"irm {WindowsScriptUrl} | iex"
        : $"curl -fsSL {LinuxScriptUrl} | sh";

    /// <summary>Starts the installer and reports what the caller must do next. Never throws.</summary>
    /// <param name="cancellationToken">A token to stop waiting for an inline installer.</param>
    /// <returns>Whether the player has to quit now, restart later, or nothing happened.</returns>
    public static async Task<UpdateLaunch> StartAsync(CancellationToken cancellationToken = default)
    {
        if (!IsSupported)
        {
            return UpdateLaunch.Failed;
        }

        try
        {
            return OperatingSystem.IsWindows()
                ? StartDetachedOnWindows()
                : await RunInlineAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // A missing shell, a blocked launch, a killed child: the player reports the manual
            // command and carries on. Failing to update is never worth taking the app down.
            return UpdateLaunch.Failed;
        }
    }

    private static UpdateLaunch StartDetachedOnWindows()
    {
        // Wait-Process first: the installer wipes the install directory, which fails while this
        // process holds its own executable open. Read-Host last so the console with the result (or
        // the error) stays on screen instead of flashing past.
        var script =
            $"Wait-Process -Id {Environment.ProcessId} -ErrorAction SilentlyContinue; " +
            $"{Command}; " +
            "Write-Host ''; Read-Host 'Press Enter to close'";

        var start = new ProcessStartInfo("powershell")
        {
            // Its own console window: this process is about to exit and take the current one with it.
            UseShellExecute = true,
            ArgumentList = { "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", script },
        };

        return Process.Start(start) is null ? UpdateLaunch.Failed : UpdateLaunch.QuitToApply;
    }

    private static async Task<UpdateLaunch> RunInlineAsync(CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo("/bin/sh")
        {
            // Inherit the terminal so the installer's own progress is what the user watches.
            UseShellExecute = false,
            ArgumentList = { "-c", Command },
        };

        using var process = Process.Start(start);
        if (process is null)
        {
            return UpdateLaunch.Failed;
        }

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        return process.ExitCode == 0 ? UpdateLaunch.RestartToApply : UpdateLaunch.Failed;
    }
}
