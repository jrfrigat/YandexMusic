using YandexMusic;

namespace YandexMusic.Tests.Integration;

/// <summary>
/// Shared fixture for live integration tests: reads <c>YANDEX_MUSIC_TOKEN</c> and creates a single
/// signed-in client reused across the test class. When no token is set, <see cref="Client"/> is
/// <see langword="null"/> and the <see cref="IntegrationFactAttribute"/> skips the tests.
/// </summary>
public sealed class YandexMusicFixture : IDisposable
{
    /// <summary>Initializes the fixture, signing in when a token is available.</summary>
    public YandexMusicFixture()
    {
        var token = Environment.GetEnvironmentVariable(IntegrationFactAttribute.TokenVariable);
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        Client = new YandexMusicClient();
        Client.Authentication.SignInWithToken(token);
    }

    /// <summary>The signed-in client, or <see langword="null"/> when no token is configured.</summary>
    public YandexMusicClient? Client { get; }

    /// <summary>Disposes the client.</summary>
    public void Dispose() => Client?.Dispose();
}
