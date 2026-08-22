using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using YandexMusic.Quasar.Mdns;

namespace YandexMusic.Quasar;

/// <summary>
/// Discovers Yandex speakers with mDNS/DNS-SD. Devices register as <c>_yandexio._tcp</c> and answer
/// with the host, port and attributes needed to reach them.
///
/// The scan queries <b>every</b> network interface separately, and that is not incidental: a socket
/// bound to <c>0.0.0.0</c> sends its multicast out whichever adapter wins on route metric, which on
/// a machine with Hyper-V, WSL or a VPN is regularly an adapter with nothing behind it. Binding one
/// socket per interface is the difference between finding every speaker and finding none.
/// </summary>
public sealed class LocalDeviceScanner : ILocalDeviceScanner
{
    private static readonly IPAddress MulticastAddress = IPAddress.Parse("224.0.0.251");
    private const int MulticastPort = 5353;

    private readonly LocalDeviceScannerOptions _options;

    /// <summary>Creates a scanner with the default options.</summary>
    public LocalDeviceScanner()
        : this(new LocalDeviceScannerOptions())
    {
    }

    /// <summary>Creates a scanner.</summary>
    /// <param name="options">The scan options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public LocalDeviceScanner(LocalDeviceScannerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<LocalDevice> DiscoverAsync(
        TimeSpan window,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "The scan window must be positive.");
        }

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(window);

        var datagrams = Channel.CreateUnbounded<(byte[] Data, IPAddress Source)>();
        var sockets = OpenSockets();

        if (sockets.Count == 0)
        {
            yield break;
        }

        var pump = Task.WhenAll(sockets.Select(socket => ReceiveAsync(socket, datagrams.Writer, deadline.Token)));
        _ = pump.ContinueWith(
            _ => datagrams.Writer.TryComplete(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        var query = Task.Run(() => AskAsync(sockets, deadline.Token), CancellationToken.None);
        var state = new ScanState(_options.ServiceType);

        try
        {
            await foreach (var (data, source) in datagrams.Reader.ReadAllAsync(CancellationToken.None).ConfigureAwait(false))
            {
                foreach (var device in state.Absorb(DnsMessage.ReadRecords(data), source))
                {
                    yield return device;
                }

                // A PTR answer names an instance but carries none of its detail; ask for the rest.
                foreach (var instance in state.TakePendingInstances())
                {
                    Send(sockets, DnsMessage.BuildQuery(instance, DnsRecordType.Any));
                }
            }
        }
        finally
        {
            await deadline.CancelAsync().ConfigureAwait(false);
            foreach (var socket in sockets)
            {
                socket.Client.Dispose();
            }

            await Task.WhenAll(pump, query).WaitAsync(TimeSpan.FromSeconds(2), CancellationToken.None)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private static List<UdpClient> OpenSockets()
    {
        var sockets = new List<UdpClient>();

        foreach (var address in LocalAddresses())
        {
            UdpClient? client = null;
            try
            {
                client = new UdpClient(AddressFamily.InterNetwork);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.Bind(new IPEndPoint(address, 0));
                client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, address.GetAddressBytes());
                client.JoinMulticastGroup(MulticastAddress, address);
                sockets.Add(client);
            }
            catch (SocketException)
            {
                // An interface that cannot carry multicast is not a failure of the scan; the others
                // still answer, and a machine with no usable interface simply finds nothing.
                client?.Dispose();
            }
        }

        return sockets;
    }

    private static IEnumerable<IPAddress> LocalAddresses()
    {
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (adapter.OperationalStatus != OperationalStatus.Up ||
                adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                !adapter.SupportsMulticast)
            {
                continue;
            }

            foreach (var address in adapter.GetIPProperties().UnicastAddresses)
            {
                if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(address.Address))
                {
                    yield return address.Address;
                }
            }
        }
    }

    private async Task AskAsync(List<UdpClient> sockets, CancellationToken cancellationToken)
    {
        var query = DnsMessage.BuildQuery(_options.ServiceType, DnsRecordType.Ptr);

        while (!cancellationToken.IsCancellationRequested)
        {
            Send(sockets, query);

            if (_options.RepeatInterval <= TimeSpan.Zero)
            {
                return;
            }

            try
            {
                await Task.Delay(_options.RepeatInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private static void Send(List<UdpClient> sockets, byte[] query)
    {
        var endpoint = new IPEndPoint(MulticastAddress, MulticastPort);
        foreach (var socket in sockets)
        {
            try
            {
                _ = socket.Send(query, query.Length, endpoint);
            }
            catch (Exception exception) when (exception is SocketException or ObjectDisposedException)
            {
                // The interface went away mid-scan, or the scan is shutting down.
            }
        }
    }

    private static async Task ReceiveAsync(
        UdpClient socket,
        ChannelWriter<(byte[] Data, IPAddress Source)> writer,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is OperationCanceledException or SocketException or ObjectDisposedException)
            {
                return;
            }

            await writer.WriteAsync((result.Buffer, result.RemoteEndPoint.Address), CancellationToken.None).ConfigureAwait(false);
        }
    }
}
