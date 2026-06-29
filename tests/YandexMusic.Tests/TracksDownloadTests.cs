using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the track download-info model and the signed direct-link resolver.</summary>
public sealed class TracksDownloadTests
{
    [Fact]
    public void Deserializes_DownloadInfo()
    {
        const string json =
            """
            { "result": [
              { "codec": "mp3", "bitrateInKbps": 192, "gain": false, "preview": false,
                "downloadInfoUrl": "https://storage.mds.yandex.net/get-info?x=1", "direct": false },
              { "codec": "mp3", "bitrateInKbps": 320, "gain": true, "preview": false,
                "downloadInfoUrl": "https://storage.mds.yandex.net/get-info?x=2", "direct": false }
            ] }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<List<DownloadInfo>>>());
        var infos = response!.Result!;

        Assert.Equal(2, infos.Count);
        Assert.Equal("mp3", infos[1].Codec);
        Assert.Equal(320, infos[1].BitrateInKbps);
        Assert.True(infos[1].Gain);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms",
        Justification = "Independently reproduces the Yandex protocol checksum to verify the resolver; not a security primitive.")]
    public void BuildDirectLink_SignsGranuleCorrectly()
    {
        const string host = "s1.downloads.example.net";
        const string path = "/abc/def/track.mp3";
        const string ts = "1700000000";
        const string s = "saltvalue123";

        var xml = $"<download-info><host>{host}</host><path>{path}</path><ts>{ts}</ts><region>225</region><s>{s}</s></download-info>";
        var url = DownloadLinkResolver.BuildDirectLink(xml);

        // Independently reproduce the documented signature: md5(salt + path-without-leading-slash + s).
        var expectedSign = Convert.ToHexString(
            MD5.HashData(Encoding.UTF8.GetBytes("XGRlBW9FXlekgbPrRHuSiA" + path[1..] + s))).ToLowerInvariant();

        Assert.Equal($"https://{host}/get-mp3/{expectedSign}/{ts}{path}", url);
        Assert.Matches("^[0-9a-f]{32}$", expectedSign);
    }

    [Fact]
    public void BuildDirectLink_ThrowsOnMissingFields()
    {
        const string xml = "<download-info><host>h</host></download-info>";
        Assert.Throws<YandexMusicSerializationException>(() => DownloadLinkResolver.BuildDirectLink(xml));
    }
}
