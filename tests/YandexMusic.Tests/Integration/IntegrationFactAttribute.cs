using Xunit;

namespace YandexMusic.Tests.Integration;

/// <summary>
/// A <see cref="FactAttribute"/> that runs only when the <c>YANDEX_MUSIC_TOKEN</c> environment
/// variable is set; otherwise the test is skipped. Used for tests that hit the live Yandex Music API.
/// </summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    /// <summary>The environment variable that supplies the OAuth token for live tests.</summary>
    public const string TokenVariable = "YANDEX_MUSIC_TOKEN";

    /// <summary>Initializes the attribute, skipping the test when no token is configured.</summary>
    public IntegrationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TokenVariable)))
        {
            Skip = $"Set {TokenVariable} to run integration tests against the live Yandex Music API.";
        }
    }
}
