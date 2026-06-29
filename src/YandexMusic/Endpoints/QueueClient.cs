using System.Globalization;
using System.Text;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Queue;
using YandexMusic.Serialization;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IQueueClient"/> implementation.</summary>
internal sealed class QueueClient : IQueueClient
{
    // The device the queue is bound to is sent as a header. A stable default keeps a queue addressable
    // across calls; callers can override it to target a specific device.
    private const string DefaultDevice =
        "os=YandexMusicApi; os_version=; manufacturer=YandexMusicApi; model=YandexMusicApi; clid=; device_id=YandexMusicApi; uuid=";

    private const string DeviceHeader = "X-Yandex-Music-Device";

    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new queue endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public QueueClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QueueItem>> GetQueuesAsync(string? device = null, CancellationToken cancellationToken = default)
    {
        var result = await _connection
            .GetAsync<QueuesListResult>("/queues", DeviceHeaders(device), cancellationToken)
            .ConfigureAwait(false);

        return result?.Queues ?? [];
    }

    /// <inheritdoc />
    public Task<Queue?> GetAsync(string queueId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueId);
        return _connection.GetAsync<Queue>($"/queues/{Uri.EscapeDataString(queueId)}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePositionAsync(string queueId, int currentIndex, string? device = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueId);

        var url = $"/queues/{Uri.EscapeDataString(queueId)}/update-position" +
                  $"?currentIndex={currentIndex.ToString(CultureInfo.InvariantCulture)}";

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["isInteractive"] = "False",
        });

        var result = await _connection
            .PostAsync<QueueUpdateResult>(url, content, DeviceHeaders(device), cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result?.Status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<string?> CreateAsync(Queue queue, string? device = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queue);

        var body = JsonSerializer.Serialize(queue, YandexMusicJson.TypeInfo<Queue>());
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        var result = await _connection
            .PostAsync<QueueCreateResult>("/queues", content, DeviceHeaders(device), cancellationToken)
            .ConfigureAwait(false);

        return result?.Id;
    }

    private static Dictionary<string, string> DeviceHeaders(string? device)
        => new() { [DeviceHeader] = string.IsNullOrEmpty(device) ? DefaultDevice : device };
}
