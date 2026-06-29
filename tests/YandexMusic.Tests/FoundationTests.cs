using YandexMusic;
using YandexMusic.Authentication;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Unit tests for the client foundation: construction, authentication and session round-tripping.</summary>
public sealed class FoundationTests
{
    [Fact]
    public void NewClient_IsNotAuthenticated()
    {
        using var client = new YandexMusicClient();
        Assert.False(client.Authentication.IsAuthenticated);
    }

    [Fact]
    public void SignInWithToken_MarksClientAuthenticated()
    {
        using var client = new YandexMusicClient();
        client.Authentication.SignInWithToken("test-token");

        Assert.True(client.Authentication.IsAuthenticated);
        Assert.Equal("test-token", client.Authentication.Session.AccessToken);
    }

    [Fact]
    public void SignInWithToken_RejectsEmptyToken()
    {
        using var client = new YandexMusicClient();
        Assert.Throws<ArgumentException>(() => client.Authentication.SignInWithToken("  "));
    }

    [Fact]
    public void SignOut_ClearsToken()
    {
        using var client = new YandexMusicClient();
        client.Authentication.SignInWithToken("test-token");
        client.Authentication.SignOut();

        Assert.False(client.Authentication.IsAuthenticated);
    }

    [Fact]
    public void AuthSession_ExportImport_RoundTrips()
    {
        var session = new AuthSession("device-42");
        session.SetAccessToken("token-xyz");
        session.Cookies.Add(new System.Net.Cookie("Session_id", "abc", "/", ".yandex.ru"));

        var snapshot = session.Export();

        var restored = new AuthSession();
        restored.Import(snapshot);

        Assert.Equal("token-xyz", restored.AccessToken);
        Assert.Equal("device-42", restored.DeviceId);
        Assert.True(restored.IsAuthenticated);
        Assert.Single(restored.Cookies.GetAllCookies());
    }

    [Fact]
    public void Options_HaveSensibleDefaults()
    {
        var options = new YandexMusicClientOptions();
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.Equal("csharp", options.DeviceId);
        Assert.Equal("ru", options.Language);
    }
}
