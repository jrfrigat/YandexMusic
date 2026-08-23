using System.Text.Json.Serialization;

namespace YandexMusic.Quasar;

/// <summary>
/// A device registered to the account, as the Quasar backend describes it. The backend knows things
/// the local network cannot tell you — above all the name the owner gave the speaker, and the
/// certificate it is supposed to present — so this is the other half of local control.
/// </summary>
/// <remarks>
/// The backend returns a great deal more per device than is bound here: the household's coordinates,
/// its Wi-Fi name, alarm and equalizer settings. None of it belongs to playback, so none of it is
/// read.
/// </remarks>
public sealed record QuasarDevice
{
    /// <summary>The device identifier. It is the same value mDNS advertises as <c>deviceId</c>.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The name the owner gave the device, for example "Kitchen".</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The hardware model key, for example <c>yandexmini</c> or <c>yandexstation_2</c>.</summary>
    public string Platform { get; init; } = string.Empty;

    /// <summary>Where the device was last seen on a local network, or <see langword="null"/> if never.</summary>
    public QuasarNetworkInfo? NetworkInfo { get; init; }

    /// <summary>The local-control material, including the certificate needed to verify the device.</summary>
    public QuasarGlagol? Glagol { get; init; }
}

/// <summary>Where a device was last reachable on a local network.</summary>
public sealed record QuasarNetworkInfo
{
    /// <summary>The port the device serves local control on.</summary>
    [JsonPropertyName("external_port")]
    public int ExternalPort { get; init; }

    /// <summary>The addresses the device reported for itself.</summary>
    [JsonPropertyName("ip_addresses")]
    public IReadOnlyList<string> IpAddresses { get; init; } = [];

    /// <summary>When the device last reported this, as a Unix time in seconds.</summary>
    [JsonPropertyName("ts")]
    public long ReportedAt { get; init; }
}

/// <summary>The local-control section of a device description.</summary>
public sealed record QuasarGlagol
{
    /// <summary>The device's TLS material.</summary>
    public QuasarGlagolSecurity? Security { get; init; }
}

/// <summary>
/// What the backend publishes about a device's TLS identity. Only the certificate is bound: the
/// backend also returns the device's private key, which nothing here needs and which this library
/// therefore never reads, stores or exposes.
/// </summary>
public sealed record QuasarGlagolSecurity
{
    /// <summary>The PEM-encoded certificate the device is expected to present.</summary>
    [JsonPropertyName("server_certificate")]
    public string ServerCertificate { get; init; } = string.Empty;
}

/// <summary>The envelope returned by the device list endpoint.</summary>
internal sealed record QuasarDeviceListResponse
{
    public string Status { get; init; } = string.Empty;

    public IReadOnlyList<QuasarDevice> Devices { get; init; } = [];
}

/// <summary>The envelope returned by the per-device token endpoint.</summary>
internal sealed record QuasarTokenResponse
{
    public string Status { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;
}
