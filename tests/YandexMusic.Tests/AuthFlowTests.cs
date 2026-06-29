using YandexMusic.Authentication;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Unit tests for the pure parsing helpers of the Passport sign-in flow. The network flows
/// themselves are best-effort and must be validated against the live Yandex Passport.
/// </summary>
public sealed class AuthFlowTests
{
    [Theory]
    [InlineData("https://music.yandex.ru/#access_token=ABC123&token_type=bearer&expires_in=100", "ABC123")]
    [InlineData("https://music.yandex.ru/oauth#token_type=bearer&access_token=XYZ", "XYZ")]
    [InlineData("https://music.yandex.ru/?error=access_denied", null)]
    [InlineData(null, null)]
    public void ExtractAccessToken_ReadsFragment(string? location, string? expected)
    {
        Assert.Equal(expected, PassportAuthenticator.ExtractAccessToken(location));
    }

    [Fact]
    public void ExtractAccessToken_UnescapesValue()
    {
        var token = PassportAuthenticator.ExtractAccessToken("https://x/#access_token=a%2Bb%2Fc&token_type=bearer");
        Assert.Equal("a+b/c", token);
    }

    [Theory]
    [InlineData("""{ "status": "ok" }""", true)]
    [InlineData("""{ "status": "ok", "errors": [] }""", true)]
    [InlineData("""{ "status": "wait" }""", false)]
    [InlineData("""{ "errors": [ "track.not_found" ] }""", false)]
    [InlineData("not json", false)]
    public void IsQrConfirmed_DetectsCompletion(string json, bool expected)
    {
        Assert.Equal(expected, PassportAuthenticator.IsQrConfirmed(json));
    }
}
