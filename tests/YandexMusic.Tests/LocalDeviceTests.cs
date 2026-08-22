using System.Net;
using YandexMusic.Quasar;
using YandexMusic.Quasar.Mdns;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Guards the mDNS reader against the wire, using a datagram laid out exactly like the ones real
/// speakers send: a name compressed back into an earlier record, an SRV port whose high byte is not
/// zero, a multi-string TXT and a trailing A record. The names and identifiers are synthetic.
/// </summary>
public sealed class LocalDeviceTests
{
    private const string ServiceType = "_yandexio._tcp.local";

    /// <summary>
    /// A speaker's answer, byte for byte. Offsets matter, because two records point back into it:
    /// the TXT name is a pointer to offset 12, and the A name is a pointer to offset 67, which sits
    /// inside the SRV record's data.
    /// </summary>
    private static byte[] Packet() => Convert.FromHexString(string.Concat(
        // [0] header: no questions, three answers
        "0000", "8400", "0000", "0003", "0000", "0000",
        // [12] the instance name in full: Speaker-TEST0001._yandexio._tcp.local
        "10", "537065616B65722D5445535430303031",
        "09", "5F79616E646578696F",
        "04", "5F746370",
        "05", "6C6F63616C",                                     // the "local" label starts at [44]
        "00",
        // [51] SRV: priority 0, weight 0, port 1961, target "spk-TEST0001" + a pointer to "local"
        "0021", "0001", "0000000A", "0015",
        "0000", "0000", "07A9",
        "0C", "73706B2D5445535430303031", "C02C",               // the target label starts at [67]
        // [82] TXT for the instance, its name compressed to [12]
        "C00C", "0010", "0001", "0000000A", "0032",
        "11", "64657669636549643D5445535430303031",             // deviceId=TEST0001
        "13", "706C6174666F726D3D79616E6465786D696E69",         // platform=yandexmini
        "0B", "636C75737465723D796573",                         // cluster=yes
        // [144] A for the SRV target, its name compressed to [67]
        "C043", "0001", "0001", "0000000A", "0004", "C0A80164"));

    [Fact]
    public void ReadRecords_ReadsTheSrvPortAcrossBothBytes()
    {
        // The regression this pins down: drop the high byte and 1961 becomes 169, so every
        // connection afterwards goes to a port nothing is listening on.
        var srv = Assert.Single(DnsMessage.ReadRecords(Packet()), record => record.Type == DnsRecordType.Srv);

        Assert.Equal(1961, srv.Port);
        Assert.Equal("spk-TEST0001.local", srv.Target);
    }

    [Fact]
    public void ReadRecords_ResolvesCompressedNames()
    {
        var records = DnsMessage.ReadRecords(Packet());

        var srv = Assert.Single(records, record => record.Type == DnsRecordType.Srv);
        var txt = Assert.Single(records, record => record.Type == DnsRecordType.Txt);
        var a = Assert.Single(records, record => record.Type == DnsRecordType.A);

        Assert.Equal("Speaker-TEST0001._yandexio._tcp.local", srv.Name);
        Assert.Equal("Speaker-TEST0001._yandexio._tcp.local", txt.Name);
        Assert.Equal("spk-TEST0001.local", a.Name);
        Assert.Equal(IPAddress.Parse("192.168.1.100"), a.Address);
    }

    [Fact]
    public void ReadRecords_SplitsEveryTextString()
    {
        var txt = Assert.Single(DnsMessage.ReadRecords(Packet()), record => record.Type == DnsRecordType.Txt);

        Assert.Equal(["deviceId=TEST0001", "platform=yandexmini", "cluster=yes"], txt.Text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(40)]
    [InlineData(100)]
    [InlineData(150)]
    public void ReadRecords_TruncatedDatagramYieldsFewerRecordsRatherThanThrowing(int length)
    {
        // Every byte here arrives from the network unauthenticated. A short or cut-off datagram is
        // something that happens, not an exception every caller has to be ready to catch.
        var records = DnsMessage.ReadRecords(Packet()[..length]);

        Assert.True(records.Count <= 3);
    }

    [Fact]
    public void ReadRecords_DoesNotHangOnACompressionPointerLoop()
    {
        // One answer whose name is a pointer at offset 12 aiming at itself. Followed naively, it
        // never terminates.
        var loop = Convert.FromHexString(string.Concat(
            "0000", "8400", "0000", "0001", "0000", "0000",
            "C00C",
            "0021", "0001", "0000000A", "0004", "00000000"));

        var records = DnsMessage.ReadRecords(loop);

        Assert.Empty(records);
    }

    [Fact]
    public void BuildQuery_AsksOneQuestionThatCanBeSkippedOnTheWayBack()
    {
        var query = DnsMessage.BuildQuery(ServiceType, DnsRecordType.Ptr);

        Assert.Equal(1, query[5]);                          // exactly one question
        Assert.Equal(0x00, query[^4]);                      // QTYPE, high byte
        Assert.Equal((byte)DnsRecordType.Ptr, query[^3]);   // QTYPE, low byte
        Assert.Equal(0x80, query[^2]);                      // QCLASS: a unicast answer is requested
        Assert.Equal(0x01, query[^1]);                      // QCLASS: IN
        Assert.Empty(DnsMessage.ReadRecords(query));        // a question section carries no answers
    }

    [Fact]
    public void ScanState_AssemblesADeviceOnceEveryPieceHasArrived()
    {
        var state = new ScanState(ServiceType);
        var source = IPAddress.Parse("192.168.1.100");

        var device = Assert.Single(state.Absorb(DnsMessage.ReadRecords(Packet()), source));

        Assert.Equal("TEST0001", device.DeviceId);
        Assert.Equal("yandexmini", device.Platform);
        Assert.Equal(new IPEndPoint(source, 1961), device.Endpoint);
        Assert.Equal("spk-TEST0001.local", device.Host);
        Assert.Equal("yes", device.Attributes["cluster"]);
    }

    [Fact]
    public void ScanState_ReportsADeviceOnlyOnceHoweverOftenItAnswers()
    {
        // Speakers answer the repeated query every time it goes out; the list must not grow copies.
        var state = new ScanState(ServiceType);
        var source = IPAddress.Parse("192.168.1.100");
        var records = DnsMessage.ReadRecords(Packet());

        Assert.Single(state.Absorb(records, source));
        Assert.Empty(state.Absorb(records, source));
        Assert.Empty(state.Absorb(records, source));
    }

    [Fact]
    public void ScanState_WithoutAnIdentityReportsNothing()
    {
        // A device with no deviceId cannot be matched to anything later, so it is left out rather
        // than reported under an identity guessed from its host name.
        var state = new ScanState(ServiceType);
        var records = DnsMessage.ReadRecords(Packet())
            .Where(record => record.Type != DnsRecordType.Txt)
            .ToList();

        Assert.Empty(state.Absorb(records, IPAddress.Parse("192.168.1.100")));
    }

    [Fact]
    public async Task DiscoverAsync_RejectsAWindowThatCannotFindAnything()
    {
        var scanner = new LocalDeviceScanner();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await foreach (var _ in scanner.DiscoverAsync(TimeSpan.Zero))
            {
                // The enumerator has to be started for the argument to be validated.
            }
        });
    }
}
