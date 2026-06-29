using System.Text.Json;
using YandexMusic.Authentication;
using YandexMusic.Http;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the OAuth device-code models and the request signer.</summary>
public sealed class AuthenticationDeviceTests
{
    [Fact]
    public void Deserializes_DeviceCode_FromSnakeCaseJson()
    {
        const string json =
            """
            { "device_code": "abc", "user_code": "1234", "verification_url": "https://ya.ru/device",
              "expires_in": 300, "interval": 5 }
            """;

        var code = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<DeviceCode>());

        Assert.NotNull(code);
        Assert.Equal("abc", code!.Code);
        Assert.Equal("1234", code.UserCode);
        Assert.Equal("https://ya.ru/device", code.VerificationUrl);
        Assert.Equal(300, code.ExpiresIn);
        Assert.Equal(5, code.Interval);
    }

    [Fact]
    public void Deserializes_OAuthToken_FromSnakeCaseJson()
    {
        const string json =
            """
            { "access_token": "tok", "refresh_token": "ref", "expires_in": 31536000, "token_type": "bearer" }
            """;

        var token = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<OAuthToken>());

        Assert.NotNull(token);
        Assert.Equal("tok", token!.AccessToken);
        Assert.Equal("ref", token.RefreshToken);
        Assert.Equal(31536000, token.ExpiresIn);
        Assert.Equal("bearer", token.TokenType);
    }

    [Fact]
    public void Sign_IsDeterministic_ForFixedTimestamp()
    {
        var first = TrackRequestSigner.Sign("12345", 1700000000);
        var second = TrackRequestSigner.Sign("12345", 1700000000);

        Assert.Equal(1700000000, first.Timestamp);
        Assert.Equal(first.Value, second.Value);
        Assert.NotEmpty(first.Value);
    }

    [Fact]
    public void Sign_UsesOnlyTrackNumber_WhenIdHasAlbumSuffix()
    {
        var bare = TrackRequestSigner.Sign("12345", 1700000000);
        var withAlbum = TrackRequestSigner.Sign("12345:678", 1700000000);

        Assert.Equal(bare.Value, withAlbum.Value);
    }
}
