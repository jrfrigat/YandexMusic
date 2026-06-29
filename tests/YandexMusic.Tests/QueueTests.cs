using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Queue;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the Queue models, including the device listing and track-reference mapping.</summary>
public sealed class QueueTests
{
    [Fact]
    public void Deserializes_Queue_WithContextAndTracks()
    {
        const string json =
            """
            { "result": {
              "context": { "type": "playlist", "id": "123:456", "description": "My Mix" },
              "tracks": [ { "trackId": "1001", "albumId": "2002", "from": "web" } ],
              "currentIndex": 1,
              "modified": "2024-01-01T00:00:00Z",
              "id": "q-1",
              "from": "desktop_win"
            } }
            """;

        var queue = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Queue>>())!.Result;

        Assert.NotNull(queue);
        Assert.Equal("q-1", queue!.Id);
        Assert.Equal("desktop_win", queue.From);
        Assert.Equal(1, queue.CurrentIndex);
        Assert.Equal("playlist", queue.Context!.Type);
        Assert.Single(queue.Tracks);
        Assert.Equal("1001", queue.Tracks[0].TrackNumber);
        Assert.Equal("2002", queue.Tracks[0].AlbumId);
    }

    [Fact]
    public void Deserializes_QueuesList_Envelope()
    {
        const string json =
            """
            { "result": { "queues": [
              { "id": "q-1", "context": { "type": "my_music" }, "modified": "2024-01-01T00:00:00Z" },
              { "id": "q-2", "context": { "type": "radio", "id": "user:onyourwave", "description": "My Wave" }, "modified": "2024-01-02T00:00:00Z" }
            ] } }
            """;

        var result = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<QueuesListResult>>())!.Result;

        Assert.NotNull(result);
        Assert.Equal(2, result!.Queues.Count);
        Assert.Equal("q-1", result.Queues[0].Id);
        Assert.Equal("my_music", result.Queues[0].Context!.Type);
        Assert.Equal("My Wave", result.Queues[1].Context!.Description);
    }
}
