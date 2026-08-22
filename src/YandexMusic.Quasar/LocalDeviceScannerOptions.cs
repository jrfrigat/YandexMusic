namespace YandexMusic.Quasar;

/// <summary>Tuning for <see cref="LocalDeviceScanner"/>. The defaults are what real speakers answer to.</summary>
public sealed record LocalDeviceScannerOptions
{
    /// <summary>The DNS-SD service type Yandex speakers register under.</summary>
    public const string YandexServiceType = "_yandexio._tcp.local";

    /// <summary>The service type to look for.</summary>
    public string ServiceType { get; init; } = YandexServiceType;

    /// <summary>
    /// Whether to keep asking for the service type for the whole window rather than once. Answers
    /// are normally immediate; a repeat catches a device that was busy or a datagram that was lost.
    /// </summary>
    public TimeSpan RepeatInterval { get; init; } = TimeSpan.FromSeconds(2);
}
