using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using YandexMusic.Quasar.Control;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using YandexMusic.Exceptions;

namespace YandexMusic.Quasar;

/// <summary>The default <see cref="IQuasarClient"/>, talking to <c>quasar.yandex.net</c> over HTTPS.</summary>
public sealed class QuasarClient : IQuasarClient
{
    private const string DeviceListUrl = "https://quasar.yandex.net/glagol/device_list";
    private const string TokenUrl = "https://quasar.yandex.net/glagol/token";

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private int _disposed;

    /// <summary>Creates a client with its own <see cref="HttpClient"/>.</summary>
    /// <param name="accessToken">The account's OAuth access token.</param>
    /// <exception cref="ArgumentException"><paramref name="accessToken"/> is null or whitespace.</exception>
    public QuasarClient(string accessToken)
        : this(accessToken, null)
    {
    }

    /// <summary>Creates a client over a supplied <see cref="HttpClient"/>.</summary>
    /// <param name="accessToken">The account's OAuth access token.</param>
    /// <param name="httpClient">
    /// The client to send requests with, or <see langword="null"/> to create and own one. A supplied
    /// client is not disposed here.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="accessToken"/> is null or whitespace.</exception>
    public QuasarClient(string accessToken, HttpClient? httpClient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient is null;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QuasarDevice>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetAsync<QuasarDeviceListResponse>(DeviceListUrl, cancellationToken).ConfigureAwait(false);
        return response.Devices;
    }

    /// <inheritdoc />
    public async Task<string> GetDeviceTokenAsync(string deviceId, string platform, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(platform);

        var url = string.Create(
            CultureInfo.InvariantCulture,
            $"{TokenUrl}?device_id={Uri.EscapeDataString(deviceId)}&platform={Uri.EscapeDataString(platform)}");

        var response = await GetAsync<QuasarTokenResponse>(url, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(response.Token))
        {
            throw new YandexMusicQuasarException(
                $"The backend returned no token for device '{deviceId}' (status '{response.Status}').");
        }

        return response.Token;
    }

    /// <inheritdoc />
    public async Task<ILocalDeviceControl> ConnectAsync(
        QuasarDevice device,
        IPEndPoint? endpoint = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(device);

        endpoint ??= ResolveEndpoint(device);

        var certificate = ReadCertificate(device);
        var deviceToken = await GetDeviceTokenAsync(device.Id, device.Platform, cancellationToken).ConfigureAwait(false);

        return new LocalDeviceControl(device.Id, endpoint, deviceToken, certificate);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static IPEndPoint ResolveEndpoint(QuasarDevice device)
    {
        var network = device.NetworkInfo
            ?? throw new YandexMusicQuasarException(
                $"The backend knows no local address for '{device.Name}' ({device.Id}). " +
                "Devices only report one while they are reachable on a network.");

        foreach (var candidate in network.IpAddresses)
        {
            if (IPAddress.TryParse(candidate, out var address))
            {
                return new IPEndPoint(address, network.ExternalPort);
            }
        }

        throw new YandexMusicQuasarException($"The backend reported no usable address for device '{device.Id}'.");
    }

    private static X509Certificate2? ReadCertificate(QuasarDevice device)
    {
        var pem = device.Glagol?.Security?.ServerCertificate;
        if (string.IsNullOrWhiteSpace(pem))
        {
            // Without a published certificate there is nothing to pin against. The connection is
            // still possible, but it can no longer prove which speaker is on the other end.
            return null;
        }

        try
        {
            return X509Certificate2.CreateFromPem(pem);
        }
        catch (CryptographicException exception)
        {
            throw new YandexMusicQuasarException(
                $"The certificate the backend published for device '{device.Id}' could not be read.", exception);
        }
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(new Uri(url), cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            throw new YandexMusicQuasarException($"Could not reach the Quasar backend at {url}.", exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new YandexMusicQuasarException(
                    $"The Quasar backend answered {(int)response.StatusCode} for {url}.");
            }

            try
            {
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using (stream.ConfigureAwait(false))
                {
                    var value = await JsonSerializer
                        .DeserializeAsync(stream, QuasarJson.TypeInfo<T>(), cancellationToken)
                        .ConfigureAwait(false);

                    return value ?? throw new YandexMusicQuasarException($"The Quasar backend returned an empty body for {url}.");
                }
            }
            catch (JsonException exception)
            {
                throw new YandexMusicQuasarException($"The Quasar backend's answer to {url} could not be read.", exception);
            }
        }
    }
}
