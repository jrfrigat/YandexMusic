using System.Collections.Concurrent;
using YandexMusic.Models.Radio;

namespace YandexMusicTerminal.Catalog;

/// <summary>
/// Radio through sessions: a session is opened from seeds, hands out batches, and is told which
/// tracks were already received so it does not repeat them. A session the server has forgotten is
/// replaced transparently by a new one with the same seeds.
/// </summary>
public sealed partial class MusicCatalog
{
    private const string MyWaveSeed = "user:onyourwave";

    // Only the radio that is playing needs its state; starting another one drops the old entries.
    private readonly ConcurrentDictionary<string, RadioState> _radios = new();

    /// <inheritdoc />
    public Task<RadioBatch> GetMyWaveAsync(CancellationToken cancellationToken = default)
        => StartRadioAsync([MyWaveSeed], cancellationToken);

    /// <inheritdoc />
    public Task<RadioBatch> GetSimilarRadioAsync(string trackId, CancellationToken cancellationToken = default)
        => StartRadioAsync(["track:" + trackId], cancellationToken);

    /// <inheritdoc />
    public async Task<WaveOptions> GetWaveOptionsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _client.Radio.GetWaveSettingsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var waves = (settings?.Blocks ?? [])
            .SelectMany(block => block.Items ?? [])
            .Where(station => station.Id is { Type.Length: > 0, Tag.Length: > 0 })
            .Select(station => new WaveChoice(station.Name, $"{station.Id!.Type}:{station.Id.Tag}"))
            .ToList();

        // The default ("any") goes first so a plain Enter keeps the wave untuned.
        var characters = (settings?.SettingRestrictions?.Diversity?.PossibleValues ?? [])
            .Where(value => !string.IsNullOrEmpty(value.SerializedSeed))
            .OrderBy(value => value.Unspecified == true ? 0 : 1)
            .Select(value => new WaveChoice(value.Name, value.SerializedSeed!))
            .ToList();

        return new WaveOptions(waves, characters);
    }

    /// <inheritdoc />
    public async Task<RadioBatch> StartRadioAsync(IReadOnlyList<string> seeds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(seeds);

        var session = await _client.RadioSessions
            .CreateAsync(seeds, new RotorSessionOptions { IncludeTracksInResponse = true }, cancellationToken)
            .ConfigureAwait(false);

        _radios.Clear();
        var key = Guid.NewGuid().ToString("N");
        var state = new RadioState(seeds, session?.RadioSessionId ?? string.Empty);
        _radios[key] = state;
        return ToBatch(key, state, session?.Sequence, session?.BatchId);
    }

    /// <inheritdoc />
    public async Task<RadioBatch> GetNextRadioBatchAsync(string sessionKey, CancellationToken cancellationToken = default)
    {
        if (!_radios.TryGetValue(sessionKey, out var state))
        {
            return new RadioBatch([], string.Empty, string.Empty, string.Empty, sessionKey);
        }

        var next = state.SessionId.Length == 0
            ? null
            : await _client.RadioSessions.GetTracksAsync(state.SessionId, state.Received, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (next is { UnknownSession: not true, Sequence.Count: > 0 })
        {
            return ToBatch(sessionKey, state, next.Sequence, next.BatchId);
        }

        // The server forgot the session or it ran dry: carry on in a fresh one with the same seeds,
        // telling it what was already played.
        var renewed = await _client.RadioSessions
            .CreateAsync(state.Seeds, new RotorSessionOptions { IncludeTracksInResponse = true, Queue = [.. state.Received] }, cancellationToken)
            .ConfigureAwait(false);
        state.SessionId = renewed?.RadioSessionId ?? string.Empty;
        return ToBatch(sessionKey, state, renewed?.Sequence, renewed?.BatchId);
    }

    private static RadioBatch ToBatch(string key, RadioState state, IReadOnlyList<Sequence>? sequence, string? batchId)
    {
        var tracks = (sequence ?? [])
            .Select(item => item.Track)
            .Where(track => track is not null)
            .Select(track => ToTrackView(track!))
            .ToList();

        // Everything handed out counts as received, played or not: the queue only prevents repeats.
        foreach (var track in tracks)
        {
            state.Received.Add(track.AlbumId is { Length: > 0 } album ? $"{track.Id}:{album}" : track.Id);
        }

        return new RadioBatch(tracks, state.Seeds[0], batchId ?? string.Empty, state.SessionId, key);
    }

    private sealed class RadioState(IReadOnlyList<string> seeds, string sessionId)
    {
        public IReadOnlyList<string> Seeds { get; } = seeds;

        public string SessionId { get; set; } = sessionId;

        public List<string> Received { get; } = [];
    }
}
