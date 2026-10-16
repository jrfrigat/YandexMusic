using System.Security.Cryptography;
using System.Text.Json;
using YandexMusic.Endpoints;
using YandexMusic.Http;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using YandexMusic.Tests.Infrastructure;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Verifies lossless file info: the desktop request signature, the request it is attached to, the
/// response model, and the streaming AES-CTR decryption of <c>encraw</c> files. The expected values
/// were produced independently with Node.js <c>crypto</c>.
/// </summary>
public sealed class TrackFileInfoTests
{
    [Fact]
    public void FileInfoSignature_MatchesTheReferenceVector()
    {
        var signature = TrackRequestSigner.SignFileInfo("12345", "lossless", ["flac", "aac", "mp3"], "encraw", 1_700_000_000);

        Assert.Equal(1_700_000_000, signature.Timestamp);
        Assert.Equal("DUHN6UK9DE96gYMHrJl1yTGm+72kqEb7vcXjDBQtvvA", signature.Value);
    }

    [Fact]
    public async Task GetFileInfo_SignsTheRequestAndSendsTheDesktopClientHeader()
    {
        var connection = new RecordingConnection();

        await new TracksClient(connection).GetFileInfoAsync("12345", "lossless", ["flac", "mp3"], "encraw");

        Assert.Equal(HttpMethod.Get, connection.Method);
        Assert.StartsWith("/get-file-info?ts=", connection.Url);
        Assert.Contains("&trackId=12345&quality=lossless&codecs=flac%2Cmp3&transports=encraw&sign=", connection.Url);
        Assert.Equal("YandexMusicDesktopAppWindows/5.95.0", connection.Headers!["X-Yandex-Music-Client"]);
    }

    [Fact]
    public async Task GetFileInfo_DefaultsToEveryKnownCodec()
    {
        var connection = new RecordingConnection();

        await new TracksClient(connection).GetFileInfoAsync("1");

        Assert.Contains("&codecs=flac%2Cflac-mp4%2Caac-mp4%2Che-aac-mp4%2Caac%2Che-aac%2Cmp3&transports=raw&", connection.Url);
    }

    [Fact]
    public void Deserializes_FileInfo()
    {
        const string json =
            """
            { "result": { "downloadInfo": {
              "trackId": "1", "quality": "lossless", "codec": "flac-mp4", "bitrate": 0, "transport": "encraw",
              "url": "https://u", "urls": ["https://u", "https://v"], "realId": "1", "key": "00ff" } } }
            """;

        var info = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<TrackFileInfo>>())!.Result!.DownloadInfo!;

        Assert.Equal("flac-mp4", info.Codec);
        Assert.Equal(2, info.Urls.Count);
        Assert.True(info.IsEncrypted);
        Assert.False(new TrackFileDownloadInfo { Transport = "raw", Key = "00ff" }.IsEncrypted);
    }

    [Theory]
    [InlineData(5000)] // whole file in one call
    [InlineData(7)] // odd chunks that straddle both block and keystream-batch boundaries
    [InlineData(4096)]
    public void Decryptor_MatchesAes128CtrWithAZeroCounter(int chunk)
    {
        var data = new byte[5000];
        for (var i = 0; i < data.Length; i++)
        {
            data[i] = (byte)((i * 7) + 3);
        }

        using (var decryptor = new AesCtrDecryptor("000102030405060708090a0b0c0d0e0f"))
        {
            for (var offset = 0; offset < data.Length; offset += chunk)
            {
                decryptor.Transform(data.AsSpan(offset, Math.Min(chunk, data.Length - offset)));
            }
        }

        // CTR is symmetric, so transforming the plaintext yields the reference ciphertext.
        Assert.Equal("c5ab2a2f98a976b6540dc832f696bd15", Convert.ToHexString(data.AsSpan(0, 16)).ToLowerInvariant());
        Assert.Equal("3b4a2f610d1ef10f3ebce55223aeedd32263416b69f8565c707600c02f4f5138", Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant());
    }

    [Theory]
    [InlineData("zz")]
    [InlineData("0011")]
    public void Decryptor_RejectsABadKey(string key)
        => Assert.Throws<ArgumentException>(() => new AesCtrDecryptor(key));
}
