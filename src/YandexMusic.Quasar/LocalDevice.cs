using System.Net;

namespace YandexMusic.Quasar;

/// <summary>
/// A Yandex speaker answering on the current network. Everything here comes from what the device
/// broadcasts about itself, so it is available with no account, no token and no internet connection.
///
/// Note what is deliberately absent: the name the owner gave the speaker ("kitchen"). Devices do not
/// advertise it, so a UI that wants to show it has to get it from an account-level source and match
/// on <see cref="DeviceId"/>.
/// </summary>
public sealed record LocalDevice
{
    /// <summary>
    /// The device's identifier, from its <c>deviceId</c> attribute. This is the identity to match on:
    /// the host name is not reliable, because some devices advertise a generic one.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// The hardware model key, from the <c>platform</c> attribute — for example <c>yandexmini</c>,
    /// <c>yandexstation_2</c>, <c>orion</c> or <c>cucumber</c>. It is reported verbatim rather than
    /// mapped to a marketing name, because the set of keys is whatever Yandex ships next.
    /// </summary>
    public required string Platform { get; init; }

    /// <summary>The address and port to open a control connection to.</summary>
    public required IPEndPoint Endpoint { get; init; }

    /// <summary>The host name the device advertised for itself.</summary>
    public required string Host { get; init; }

    /// <summary>Every attribute the device advertised, including the ones above.</summary>
    public IReadOnlyDictionary<string, string> Attributes { get; init; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>The instance name the device registered under, kept for diagnostics.</summary>
    public required string ServiceName { get; init; }
}
