using YandexMusicTerminal.Diagnostics;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Guards the version comparison behind the player's update notice. A false positive nags every
/// launch about an update that does not exist; a false negative hides a real one. Both are quiet
/// failures nobody reports, so the edges are pinned down here.
/// </summary>
public sealed class UpdateCheckTests
{
    [Theory]
    [InlineData("0.4.0", "0.3.0")]
    [InlineData("0.4.1", "0.4.0")]
    [InlineData("1.0.0", "0.9.9")]
    [InlineData("0.10.0", "0.9.0")]
    public void IsNewer_IsTrueForALaterRelease(string candidate, string current)
        => Assert.True(UpdateChecker.IsNewer(candidate, current));

    [Theory]
    [InlineData("0.3.0", "0.4.0")]
    [InlineData("0.4.0", "0.4.0")]
    [InlineData("0.9.0", "0.10.0")]
    public void IsNewer_IsFalseForTheSameOrAnOlderRelease(string candidate, string current)
        => Assert.False(UpdateChecker.IsNewer(candidate, current));

    [Fact]
    public void IsNewer_IgnoresThePreReleaseSuffixOfALocalBuild()
    {
        // An untagged local build reports "0.4.1-preview.0.5"; the released 0.4.0 is not newer than it.
        Assert.False(UpdateChecker.IsNewer("0.4.0", "0.4.1-preview.0.5"));
        Assert.True(UpdateChecker.IsNewer("0.5.0", "0.4.1-preview.0.5"));
    }

    [Theory]
    [InlineData("", "0.4.0")]
    [InlineData("not-a-version", "0.4.0")]
    [InlineData("0.5.0", "")]
    [InlineData("v0.5.0", "0.4.0")]
    public void IsNewer_IsFalseWhenEitherSideIsUnparseable(string candidate, string current)
    {
        // Silence beats a bogus "update available" the user cannot act on.
        Assert.False(UpdateChecker.IsNewer(candidate, current));
    }

    [Fact]
    public void CurrentVersion_HasNoBuildMetadata()
    {
        // MinVer appends "+<sha>"; it must not reach a version comparison or the UI.
        Assert.DoesNotContain("+", UpdateChecker.CurrentVersion, StringComparison.Ordinal);
        Assert.NotEmpty(UpdateChecker.CurrentVersion);
    }

    [Fact]
    public async Task CheckNowAsync_DoesNotThrowWhenCancelled()
    {
        // The About screen fires this off without awaiting it, so an escaping exception would be an
        // unobserved one that takes the process down rather than an error anybody sees.
        using var checker = new UpdateChecker();
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        var status = await checker.CheckNowAsync(cancelled.Token);

        Assert.Equal(UpdateStatus.Unknown, status);
        Assert.False(checker.IsChecking);
    }

    [Fact]
    public void Updater_RunsTheInstallerForThisPlatform()
    {
        // A swapped script (the PowerShell one on Linux) fails only on a user's machine, at the
        // moment they press "update", which is the worst possible place to find out.
        Assert.True(Updater.IsSupported);
        Assert.Contains(
            OperatingSystem.IsWindows() ? "install.ps1" : "install.sh",
            Updater.Command,
            StringComparison.Ordinal);
    }
}
