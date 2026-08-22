using System.Net;

namespace YandexMusic.LocalDevices.Mdns;

/// <summary>The record types this package needs to read out of an mDNS answer.</summary>
internal enum DnsRecordType
{
    /// <summary>An IPv4 address.</summary>
    A = 1,

    /// <summary>A pointer: the service type to instance name mapping of DNS-SD.</summary>
    Ptr = 12,

    /// <summary>Key/value text attached to a service instance.</summary>
    Txt = 16,

    /// <summary>An IPv6 address.</summary>
    Aaaa = 28,

    /// <summary>The host and port a service instance lives on.</summary>
    Srv = 33,

    /// <summary>Every record for a name; used to fetch an instance's SRV, TXT and A in one exchange.</summary>
    Any = 255,
}

/// <summary>
/// One resource record from an mDNS answer. A single type covers all of them because only a handful
/// of shapes matter here and each carries at most one payload; a class per record type would be more
/// ceremony than the four fields below are worth.
/// </summary>
internal sealed record DnsRecord
{
    /// <summary>The name this record is about.</summary>
    public required string Name { get; init; }

    /// <summary>The record type.</summary>
    public required DnsRecordType Type { get; init; }

    /// <summary>The target name of a PTR or SRV record.</summary>
    public string? Target { get; init; }

    /// <summary>The port of an SRV record.</summary>
    public int Port { get; init; }

    /// <summary>The address of an A or AAAA record.</summary>
    public IPAddress? Address { get; init; }

    /// <summary>The raw strings of a TXT record, each normally <c>key=value</c>.</summary>
    public IReadOnlyList<string> Text { get; init; } = [];
}
