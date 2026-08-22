using System.Diagnostics;
using YandexMusic;
using YandexMusic.Models.Tracks;

namespace YandexMusicTerminal.Playback;

/// <summary>
/// Reports listening activity to the API so recommendations and "My Wave" react to what is played:
/// a play-audio event when a track starts, and radio feedback (radio started / track started /
/// finished / skipped) while a radio queue is playing. Failures are swallowed — reporting must
/// never break playback.
/// </summary>
public sealed class PlayReporter : IDisposable
{
    private readonly IYandexMusicClient _client;
    private readonly PlaybackController _controller;
    private readonly Stopwatch _played = new();
    private string? _reportedStation;

    /// <summary>Creates a reporter bound to a playback controller.</summary>
    /// <param name="client">The signed-in API client.</param>
    /// <param name="controller">The playback controller to observe.</param>
    public PlayReporter(IYandexMusicClient client, PlaybackController controller)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(controller);
        _client = client;
        _controller = controller;
        _controller.TrackStarted += OnTrackStarted;
        _controller.TrackLeft += OnTrackLeft;
        _controller.Changed += OnChanged;
    }

    private void OnTrackStarted(PlaybackItem item)
    {
        _played.Restart();

        // The play id travels with the report rather than through a field: the report is fire-and-
        // forget, and a quick skip would otherwise stamp this track's event with the next track's id.
        _ = ReportAsync(item, Guid.NewGuid().ToString("N"));
    }

    /// <summary>Keeps the listened-time clock honest: a paused or buffering track is not being played.</summary>
    private void OnChanged()
    {
        if (_controller.State == PlaybackState.Playing)
        {
            _played.Start();
        }
        else
        {
            _played.Stop();
        }
    }

    private void OnTrackLeft(PlaybackItem item, bool endedNaturally)
    {
        _played.Stop();
        if (item.Origin?.Station is not { Length: > 0 } station)
        {
            return;
        }

        _ = endedNaturally
            ? TryAsync(() => _client.Radio.SendStationFeedbackTrackFinishedAsync(
                station, item.Id, _played.Elapsed.TotalSeconds, item.Origin.BatchId))
            : TryAsync(() => _client.Radio.SendStationFeedbackSkipAsync(
                station, item.Id, _played.Elapsed.TotalSeconds, item.Origin.BatchId));
    }

    private async Task ReportAsync(PlaybackItem item, string playId)
    {
        var origin = item.Origin;
        await TryAsync(() => _client.Tracks.PlayAudioAsync(new PlayAudioOptions
        {
            TrackId = item.Id,
            From = origin?.From ?? "ymt",
            AlbumId = origin?.AlbumId ?? string.Empty,
            PlaylistId = origin?.PlaylistId,
            PlayId = playId,
            TrackLengthSeconds = (int)Math.Ceiling(item.Duration.TotalSeconds),
            TotalPlayedSeconds = 0,
            EndPositionSeconds = 0,
        })).ConfigureAwait(false);

        if (origin?.Station is not { Length: > 0 } station)
        {
            return;
        }

        if (_reportedStation != station)
        {
            _reportedStation = station;
            await TryAsync(() => _client.Radio.SendStationFeedbackRadioStartedAsync(
                station, origin.From, origin.BatchId)).ConfigureAwait(false);
        }

        await TryAsync(() => _client.Radio.SendStationFeedbackTrackStartedAsync(
            station, item.Id, origin.BatchId)).ConfigureAwait(false);
    }

    private static async Task TryAsync(Func<Task<bool>> call)
    {
        try
        {
            await call().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Reporting is best-effort; the player keeps going whatever the API says.
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _controller.TrackStarted -= OnTrackStarted;
        _controller.TrackLeft -= OnTrackLeft;
        _controller.Changed -= OnChanged;
    }
}
