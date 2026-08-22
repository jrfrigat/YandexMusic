using System.Net;
using YandexMusic.Quasar.Mdns;

namespace YandexMusic.Quasar;

/// <summary>
/// Assembles the records of a scan into devices. Answers arrive in pieces and out of order — a PTR
/// naming an instance, an SRV giving it a host and port, a TXT giving it an identity, an A giving
/// the host an address — so this holds the fragments until one device is complete, then releases it
/// exactly once.
/// </summary>
internal sealed class ScanState(string serviceType)
{
    private const string DeviceIdAttribute = "deviceId";
    private const string PlatformAttribute = "platform";

    private readonly Dictionary<string, DnsRecord> _services = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _attributes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IPAddress> _hosts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IPAddress> _sources = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _known = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _announced = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<string> _pending = new();

    /// <summary>Takes in one datagram's records and returns whatever devices they completed.</summary>
    /// <param name="records">The records read from the datagram.</param>
    /// <param name="source">The address the datagram came from.</param>
    /// <returns>The devices that became complete, each reported only the first time.</returns>
    public IEnumerable<LocalDevice> Absorb(IReadOnlyList<DnsRecord> records, IPAddress source)
    {
        var touched = new List<string>();

        foreach (var record in records)
        {
            switch (record.Type)
            {
                case DnsRecordType.Ptr when record.Target is { Length: > 0 } instance &&
                                            record.Name.Equals(serviceType, StringComparison.OrdinalIgnoreCase):
                    if (_known.Add(instance))
                    {
                        _pending.Enqueue(instance);
                    }

                    break;

                case DnsRecordType.Srv when record.Target is { Length: > 0 }:
                    _services[record.Name] = record;
                    // The datagram came from the device itself; keep it as the address of last
                    // resort for when no A record ever arrives for the advertised host.
                    _sources[record.Name] = source;
                    touched.Add(record.Name);
                    break;

                case DnsRecordType.Txt:
                    _attributes[record.Name] = ParseAttributes(record.Text);
                    touched.Add(record.Name);
                    break;

                case DnsRecordType.A:
                    if (record.Address is not null)
                    {
                        _hosts[record.Name] = record.Address;
                        touched.AddRange(_services.Where(pair => pair.Value.Target is { } target &&
                                                                 target.Equals(record.Name, StringComparison.OrdinalIgnoreCase))
                                                  .Select(pair => pair.Key));
                    }

                    break;

                default:
                    break;
            }
        }

        foreach (var instance in touched.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (TryBuild(instance, out var device))
            {
                yield return device;
            }
        }
    }

    /// <summary>Returns the instances discovered but not yet asked about in detail, and forgets them.</summary>
    /// <returns>The instance names to query.</returns>
    public IReadOnlyList<string> TakePendingInstances()
    {
        if (_pending.Count == 0)
        {
            return [];
        }

        var pending = new List<string>(_pending.Count);
        while (_pending.TryDequeue(out var instance))
        {
            pending.Add(instance);
        }

        return pending;
    }

    private static Dictionary<string, string> ParseAttributes(IReadOnlyList<string> entries)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            var separator = entry.IndexOf('=', StringComparison.Ordinal);
            if (separator > 0)
            {
                attributes[entry[..separator]] = entry[(separator + 1)..];
            }
        }

        return attributes;
    }

    private bool TryBuild(string instance, out LocalDevice device)
    {
        device = null!;

        if (!_services.TryGetValue(instance, out var service) ||
            service.Target is not { Length: > 0 } host ||
            !_attributes.TryGetValue(instance, out var attributes))
        {
            return false;
        }

        // Identity has to come from the attributes: some devices advertise a generic host name
        // ("Android.local"), so the host is not something to key on.
        if (!attributes.TryGetValue(DeviceIdAttribute, out var deviceId) || string.IsNullOrWhiteSpace(deviceId))
        {
            return false;
        }

        if (!_hosts.TryGetValue(host, out var address) && !_sources.TryGetValue(instance, out address))
        {
            return false;
        }

        if (!_announced.Add(deviceId))
        {
            return false;
        }

        device = new LocalDevice
        {
            DeviceId = deviceId,
            Platform = attributes.TryGetValue(PlatformAttribute, out var platform) ? platform : string.Empty,
            Endpoint = new IPEndPoint(address, service.Port),
            Host = host,
            Attributes = attributes,
            ServiceName = instance,
        };

        return true;
    }
}
