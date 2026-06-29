namespace YandexMusic.Authentication;

/// <summary>
/// Options for the OAuth device-code sign-in flow. The defaults match the public Yandex Music
/// application and are sufficient for most callers.
/// </summary>
public sealed class DeviceAuthOptions
{
    /// <summary>The OAuth client identifier. Defaults to the public Yandex Music application.</summary>
    public string ClientId { get; init; } = DeviceAuthDefaults.ClientId;

    /// <summary>The OAuth client secret. Defaults to the public Yandex Music application.</summary>
    public string ClientSecret { get; init; } = DeviceAuthDefaults.ClientSecret;

    /// <summary>
    /// The device identifier reported to the OAuth service. When <see langword="null"/>, the
    /// session's device identifier is used.
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>The human-readable device name shown to the user. Defaults to <c>YandexMusicAPI</c>.</summary>
    public string DeviceName { get; init; } = DeviceAuthDefaults.DeviceName;

    /// <summary>
    /// The interval between polling attempts. When <see langword="null"/>, the server-recommended
    /// <see cref="DeviceCode.Interval"/> is used.
    /// </summary>
    public TimeSpan? PollInterval { get; init; }

    /// <summary>
    /// The maximum time to wait for the user to confirm the sign-in. When <see langword="null"/>,
    /// the server-provided <see cref="DeviceCode.ExpiresIn"/> is used.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}

/// <summary>The built-in OAuth application defaults for the device-code flow.</summary>
internal static class DeviceAuthDefaults
{
    /// <summary>The public Yandex Music OAuth client identifier.</summary>
    public const string ClientId = "23cabbbdc6cd418abb4b39c32c41195d";

    /// <summary>The public Yandex Music OAuth client secret.</summary>
    public const string ClientSecret = "53bc75238f0c4d08a118e51fe9203300";

    /// <summary>The default device name reported to the OAuth service.</summary>
    public const string DeviceName = "YandexMusicAPI";
}
