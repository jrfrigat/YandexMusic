using System.Text.RegularExpressions;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Checks that every string the terminal asks for actually exists, in both languages. A missing key
/// does not throw: <c>ResourceManager</c> returns the key itself, so the UI quietly starts showing
/// "RemoteLocalNone" to the user and nothing fails until somebody looks at the screen.
/// </summary>
public sealed class StringsResourceTests
{
    private static readonly Regex Accessor = new(
        @"\bnameof\((?<key>[A-Za-z][A-Za-z0-9_]*)\)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly Regex ResourceKey = new(
        @"<data\s+name=""(?<key>[^""]+)""", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    [Fact]
    public void EveryStringTheTerminalAsksForExistsInBothLanguages()
    {
        var root = RepositoryRoot();
        var resources = Path.Combine(root, "samples", "YandexMusicTerminal", "Resources");

        var used = Keys(Accessor, Path.Combine(root, "samples", "YandexMusicTerminal", "Ui", "Strings.cs"));
        var english = Keys(ResourceKey, Path.Combine(resources, "Strings.resx"));
        var russian = Keys(ResourceKey, Path.Combine(resources, "Strings.ru.resx"));

        Assert.NotEmpty(used);
        Assert.Empty(used.Except(english).Order());
        Assert.Empty(used.Except(russian).Order());
    }

    [Fact]
    public void TheTwoLanguagesCoverTheSameKeys()
    {
        // A key translated on one side only means the other language silently falls back to English,
        // or to the key name, depending on which side is missing.
        var resources = Path.Combine(RepositoryRoot(), "samples", "YandexMusicTerminal", "Resources");

        var english = Keys(ResourceKey, Path.Combine(resources, "Strings.resx"));
        var russian = Keys(ResourceKey, Path.Combine(resources, "Strings.ru.resx"));

        Assert.Empty(english.Except(russian).Order());
        Assert.Empty(russian.Except(english).Order());
    }

    private static HashSet<string> Keys(Regex pattern, string path)
    {
        Assert.True(File.Exists(path), $"Expected to find {path}.");

        return pattern.Matches(File.ReadAllText(path))
            .Select(match => match.Groups["key"].Value)
            .Where(key => !key.StartsWith("resmimetype", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "YandexMusic.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
