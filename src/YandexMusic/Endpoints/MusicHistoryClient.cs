using System.Buffers;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.MusicHistory;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IMusicHistoryClient"/> implementation.</summary>
internal sealed class MusicHistoryClient : IMusicHistoryClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new music-history endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public MusicHistoryClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<MusicHistory?> GetAsync(int fullModelsCount = 0, CancellationToken cancellationToken = default)
    {
        var url = $"/music-history?fullModelsCount={fullModelsCount.ToString(CultureInfo.InvariantCulture)}";
        return _connection.GetAsync<MusicHistory>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MusicHistoryItems?> GetItemsAsync(IReadOnlyList<MusicHistoryItemRequest> items, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        var body = BuildItemsBody(items);
        var content = new StringContent(body, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return SendItemsAsync(content, cancellationToken);
    }

    private async Task<MusicHistoryItems?> SendItemsAsync(StringContent content, CancellationToken cancellationToken)
    {
        using (content)
        {
            return await _connection
                .PostAsync<MusicHistoryItems>("/music-history/items", content, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static string BuildItemsBody(IReadOnlyList<MusicHistoryItemRequest> items)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteStartArray("items");

            foreach (var item in items)
            {
                if (item is null)
                {
                    continue;
                }

                writer.WriteStartObject();
                writer.WriteString("type", item.Type);
                writer.WriteStartObject("data");
                writer.WriteStartObject("itemId");
                WriteItemId(writer, item.Data);
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static void WriteItemId(Utf8JsonWriter writer, MusicHistoryItemRequestData data)
    {
        if (data is null)
        {
            return;
        }

        if (data.TrackId is not null)
        {
            writer.WriteString("trackId", data.TrackId);
        }

        if (data.AlbumId is not null)
        {
            writer.WriteString("albumId", data.AlbumId);
        }

        if (data.Id is not null)
        {
            writer.WriteString("id", data.Id);
        }

        if (data.Uid is { } uid)
        {
            writer.WriteNumber("uid", uid);
        }

        if (data.Kind is { } kind)
        {
            writer.WriteNumber("kind", kind);
        }

        if (data.Seeds is { } seeds)
        {
            writer.WriteStartArray("seeds");
            foreach (var seed in seeds)
            {
                writer.WriteStringValue(seed);
            }

            writer.WriteEndArray();
        }
    }
}
