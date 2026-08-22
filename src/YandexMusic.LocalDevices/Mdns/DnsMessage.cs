using System.Net;

namespace YandexMusic.LocalDevices.Mdns;

/// <summary>
/// The sliver of the DNS wire format that DNS-SD discovery needs: build one question, and read the
/// answers back. This is deliberately hand-rolled rather than taken from a package — a PTR query
/// plus PTR/SRV/TXT/A parsing is all that is ever needed here, and the core of this library
/// advertises that it depends on nothing but the BCL.
///
/// Everything parsed here arrives from the network unauthenticated, so every read is bounds-checked
/// and a malformed packet yields fewer records rather than an exception.
/// </summary>
internal static class DnsMessage
{
    private const int HeaderLength = 12;
    private const int MaxNameLabels = 128;
    private const byte PointerMask = 0xC0;

    /// <summary>Builds a query for one name.</summary>
    /// <param name="name">The name to ask about, for example <c>_yandexio._tcp.local</c>.</param>
    /// <param name="type">The record type to ask for.</param>
    /// <returns>The datagram to send to the mDNS group.</returns>
    public static byte[] BuildQuery(string name, DnsRecordType type)
    {
        var labels = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var length = HeaderLength + labels.Sum(label => label.Length + 1) + 1 + 4;
        var buffer = new byte[length];

        // Transaction id 0 and no flags: mDNS matches answers by question, not by id.
        buffer[5] = 1; // one question

        var offset = HeaderLength;
        foreach (var label in labels)
        {
            var encoded = System.Text.Encoding.UTF8.GetBytes(label);
            if (encoded.Length > 63)
            {
                throw new ArgumentException($"The label '{label}' is longer than the 63 bytes DNS allows.", nameof(name));
            }

            buffer[offset++] = (byte)encoded.Length;
            encoded.CopyTo(buffer, offset);
            offset += encoded.Length;
        }

        buffer[offset++] = 0;
        buffer[offset++] = (byte)((int)type >> 8);
        buffer[offset++] = (byte)((int)type & 0xFF);
        // Class IN with the unicast-response bit set, so answers come straight back to this socket
        // instead of only to the multicast group.
        buffer[offset++] = 0x80;
        buffer[offset] = 0x01;

        return buffer;
    }

    /// <summary>Reads every answer, authority and additional record out of a response.</summary>
    /// <param name="buffer">The received datagram.</param>
    /// <returns>The records that could be read; empty when the datagram is not usable.</returns>
    public static IReadOnlyList<DnsRecord> ReadRecords(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        var records = new List<DnsRecord>();
        if (buffer.Length < HeaderLength)
        {
            return records;
        }

        var questions = ReadUInt16(buffer, 4);
        var count = ReadUInt16(buffer, 6) + ReadUInt16(buffer, 8) + ReadUInt16(buffer, 10);

        var offset = HeaderLength;
        for (var i = 0; i < questions; i++)
        {
            if (!TryReadName(buffer, offset, out _, out var consumed))
            {
                return records;
            }

            offset += consumed + 4;
        }

        for (var i = 0; i < count; i++)
        {
            if (!TryReadName(buffer, offset, out var name, out var consumed))
            {
                return records;
            }

            offset += consumed;
            if (offset + 10 > buffer.Length)
            {
                return records;
            }

            var type = (DnsRecordType)ReadUInt16(buffer, offset);
            var dataLength = ReadUInt16(buffer, offset + 8);
            var dataOffset = offset + 10;
            offset = dataOffset + dataLength;
            if (offset > buffer.Length)
            {
                return records;
            }

            var record = ReadRecord(buffer, name, type, dataOffset, dataLength);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return records;
    }

    private static DnsRecord? ReadRecord(byte[] buffer, string name, DnsRecordType type, int dataOffset, int dataLength)
    {
        switch (type)
        {
            case DnsRecordType.Ptr:
                return TryReadName(buffer, dataOffset, out var target, out _)
                    ? new DnsRecord { Name = name, Type = type, Target = target }
                    : null;

            case DnsRecordType.Srv:
                // priority (2) + weight (2) + port (2) + target.
                if (dataLength < 7 || !TryReadName(buffer, dataOffset + 6, out var host, out _))
                {
                    return null;
                }

                return new DnsRecord
                {
                    Name = name,
                    Type = type,
                    Target = host,
                    Port = ReadUInt16(buffer, dataOffset + 4),
                };

            case DnsRecordType.Txt:
                return new DnsRecord { Name = name, Type = type, Text = ReadTextStrings(buffer, dataOffset, dataLength) };

            case DnsRecordType.A when dataLength == 4:
            case DnsRecordType.Aaaa when dataLength == 16:
                var address = new byte[dataLength];
                Array.Copy(buffer, dataOffset, address, 0, dataLength);
                return new DnsRecord { Name = name, Type = type, Address = new IPAddress(address) };

            default:
                return null;
        }
    }

    private static List<string> ReadTextStrings(byte[] buffer, int offset, int length)
    {
        var entries = new List<string>();
        var end = Math.Min(offset + length, buffer.Length);
        var position = offset;

        while (position < end)
        {
            var size = buffer[position++];
            if (size == 0 || position + size > end)
            {
                break;
            }

            entries.Add(System.Text.Encoding.UTF8.GetString(buffer, position, size));
            position += size;
        }

        return entries;
    }

    /// <summary>
    /// Reads a name, following compression pointers. <paramref name="consumed"/> counts only the
    /// bytes at <paramref name="offset"/> itself: a pointer is two bytes long however far it jumps.
    /// </summary>
    private static bool TryReadName(byte[] buffer, int offset, out string name, out int consumed)
    {
        name = string.Empty;
        consumed = 0;

        var labels = new List<string>();
        var position = offset;
        var jumped = false;
        var guard = 0;

        while (position < buffer.Length)
        {
            if (++guard > MaxNameLabels)
            {
                // A pointer loop, whether malicious or corrupt. Refuse the name rather than spin.
                return false;
            }

            var length = buffer[position];
            if (length == 0)
            {
                position++;
                if (!jumped)
                {
                    consumed = position - offset;
                }

                name = string.Join('.', labels);
                return true;
            }

            if ((length & PointerMask) == PointerMask)
            {
                if (position + 1 >= buffer.Length)
                {
                    return false;
                }

                var pointer = ((length & 0x3F) << 8) | buffer[position + 1];
                if (!jumped)
                {
                    consumed = position + 2 - offset;
                }

                jumped = true;
                position = pointer;
                continue;
            }

            position++;
            if (position + length > buffer.Length)
            {
                return false;
            }

            labels.Add(System.Text.Encoding.UTF8.GetString(buffer, position, length));
            position += length;
        }

        return false;
    }

    private static int ReadUInt16(byte[] buffer, int offset)
        => offset + 1 < buffer.Length ? (buffer[offset] << 8) | buffer[offset + 1] : 0;
}
