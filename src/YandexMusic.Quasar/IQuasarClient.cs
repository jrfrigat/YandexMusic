namespace YandexMusic.Quasar;

/// <summary>
/// Talks to the Quasar backend about the account's devices. Two things live here that the local
/// network cannot provide: the name the owner gave each speaker, and the certificate that speaker is
/// supposed to present, without which a local connection can only be trusted blindly.
/// </summary>
/// <remarks>
/// These endpoints are undocumented and outside the Music API. They are in this package rather than
/// the core precisely because a consumer of the REST API should never be dragged into them.
/// </remarks>
public interface IQuasarClient : IDisposable
{
    /// <summary>
    /// Lists every device registered to the account — which is more than speakers: cameras and the
    /// phone apps signed in to the account come back too. Filter on
    /// <see cref="QuasarDevice.NetworkInfo"/> and <see cref="QuasarDevice.Platform"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The account's devices.</returns>
    /// <exception cref="YandexMusicQuasarException">The backend refused the request or answered unusably.</exception>
    Task<IReadOnlyList<QuasarDevice>> GetDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtains the token a device requires inside every command message. The token is specific to one
    /// device and is not interchangeable between them.
    /// </summary>
    /// <param name="deviceId">The device's identifier.</param>
    /// <param name="platform">The device's hardware platform.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The token to put in a message's <c>conversationToken</c>.</returns>
    /// <exception cref="YandexMusicQuasarException">The backend refused the request or answered unusably.</exception>
    Task<string> GetDeviceTokenAsync(string deviceId, string platform, CancellationToken cancellationToken = default);
}
