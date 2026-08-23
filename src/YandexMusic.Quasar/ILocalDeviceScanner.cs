namespace YandexMusic.Quasar;

/// <summary>
/// Finds Yandex speakers on the current network. Discovery is purely local: no account, no token and
/// no internet connection are involved, which is also why it can only report what a device
/// broadcasts about itself.
/// </summary>
public interface ILocalDeviceScanner
{
    /// <summary>
    /// Listens for devices for <paramref name="window"/> and yields each one as it answers.
    /// A device that answers twice is yielded once.
    /// </summary>
    /// <param name="window">How long to keep listening. Answers usually arrive within a second.</param>
    /// <param name="cancellationToken">A token to stop the scan early.</param>
    /// <returns>
    /// The devices, streamed rather than collected: a caller showing a list wants each speaker on
    /// screen the moment it replies, not after the whole window has elapsed.
    /// </returns>
    /// <remarks>
    /// Finding nothing is a normal outcome, not an error. Many corporate and guest networks block
    /// multicast outright, and the scan simply ends empty when they do.
    /// </remarks>
    IAsyncEnumerable<LocalDevice> DiscoverAsync(TimeSpan window, CancellationToken cancellationToken = default);
}
