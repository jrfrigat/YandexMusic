using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Account;
using YandexMusic.Models.Radio;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IRadioClient"/> implementation.</summary>
internal sealed class RadioClient : IRadioClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new radio endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public RadioClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<AccountStatus?> GetAccountStatusAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<AccountStatus>("/rotor/account/status", cancellationToken);

    /// <inheritdoc />
    public Task<Dashboard?> GetStationsDashboardAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<Dashboard>("/rotor/stations/dashboard", cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StationResult>> GetStationsListAsync(string? language = null, CancellationToken cancellationToken = default)
    {
        var url = "/rotor/stations/list";
        if (!string.IsNullOrEmpty(language))
        {
            url += $"?language={Uri.EscapeDataString(language)}";
        }

        var stations = await _connection
            .GetAsync<List<StationResult>>(url, cancellationToken)
            .ConfigureAwait(false);

        return stations ?? (IReadOnlyList<StationResult>)[];
    }

    /// <inheritdoc />
    public async Task<bool> SendStationFeedbackAsync(
        string station,
        string type,
        DateTimeOffset? timestamp = null,
        string? from = null,
        string? batchId = null,
        double? totalPlayedSeconds = null,
        string? trackId = null,
        string? streamId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        // The endpoint takes a JSON body; form fields are rejected. A zero play time is the same as
        // none, so it is left out rather than reported as an instant skip.
        using var content = JsonBody.FromObject(writer =>
        {
            writer.WriteString("type", type);
            writer.WriteString("timestamp", JsonBody.FormatTimestamp(timestamp));

            if (!string.IsNullOrEmpty(trackId))
            {
                writer.WriteString("trackId", trackId);
            }

            if (!string.IsNullOrEmpty(from))
            {
                writer.WriteString("from", from);
            }

            if (totalPlayedSeconds is { } played && played != 0)
            {
                writer.WriteNumber("totalPlayedSeconds", played);
            }
        });

        var url = FeedbackUrl(station, batchId, streamId);
        var result = await _connection
            .PostAsync<string>(url, content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<bool> SendStationFeedbackRadioStartedAsync(
        string station,
        string from,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        return SendStationFeedbackAsync(station, "radioStarted", timestamp, from, batchId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendStationFeedbackTrackStartedAsync(
        string station,
        string trackId,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return SendStationFeedbackAsync(station, "trackStarted", timestamp, batchId: batchId, trackId: trackId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendStationFeedbackTrackFinishedAsync(
        string station,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return SendStationFeedbackAsync(station, "trackFinished", timestamp, batchId: batchId, totalPlayedSeconds: totalPlayedSeconds, trackId: trackId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendStationFeedbackSkipAsync(
        string station,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return SendStationFeedbackAsync(station, "skip", timestamp, batchId: batchId, totalPlayedSeconds: totalPlayedSeconds, trackId: trackId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StationResult>> GetStationInfoAsync(string station, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);

        var stations = await _connection
            .GetAsync<List<StationResult>>($"/rotor/station/{Uri.EscapeDataString(station)}/info", cancellationToken)
            .ConfigureAwait(false);

        return stations ?? (IReadOnlyList<StationResult>)[];
    }

    /// <inheritdoc />
    public Task<GenerativeStream?> GetStationStreamAsync(string station, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        return _connection.GetAsync<GenerativeStream>($"/rotor/station/{Uri.EscapeDataString(station)}/stream", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GenerativeStreamFeedback?> SendStationStreamFeedbackAsync(
        string station,
        string type,
        string? streamId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        using var content = JsonBody.FromObject(writer =>
        {
            writer.WriteString("type", type);
            writer.WriteString("timestamp", JsonBody.FormatTimestamp(timestamp));
        });

        return await _connection
            .PostAsync<GenerativeStreamFeedback>(FeedbackUrl(station, batchId: null, streamId), content, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SetStationSettingsAsync(
        string station,
        string moodEnergy,
        string diversity,
        string type = "rotor",
        string language = "not-russian",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        ArgumentException.ThrowIfNullOrWhiteSpace(moodEnergy);
        ArgumentException.ThrowIfNullOrWhiteSpace(diversity);

        using var content = JsonBody.FromObject(writer =>
        {
            writer.WriteString("moodEnergy", moodEnergy);
            writer.WriteString("diversity", diversity);
            writer.WriteString("type", type);

            if (!string.IsNullOrEmpty(language))
            {
                writer.WriteString("language", language);
            }
        });

        var result = await _connection
            .PostAsync<string>($"/rotor/station/{Uri.EscapeDataString(station)}/settings3", content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<StationTracksResult?> GetStationTracksAsync(
        string station,
        bool settings2 = true,
        string? queue = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);

        var url = $"/rotor/station/{Uri.EscapeDataString(station)}/tracks";
        if (!string.IsNullOrEmpty(queue))
        {
            url += $"?queue={Uri.EscapeDataString(queue)}";
        }
        else if (settings2)
        {
            url += "?settings2=True";
        }

        return _connection.GetAsync<StationTracksResult>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Wave?> GetLastWaveAsync(CancellationToken cancellationToken = default)
        => _connection.GetAsync<Wave>("/rotor/wave/last", cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ResetLastWaveAsync(CancellationToken cancellationToken = default)
    {
        var result = await _connection
            .PostAsync<JsonElement>("/rotor/wave/last/reset", null, cancellationToken)
            .ConfigureAwait(false);

        // The acknowledgement is an object of the form { "result": "ok" } inside the envelope.
        return result.ValueKind == JsonValueKind.Object
            && result.TryGetProperty("result", out var status)
            && status.ValueKind == JsonValueKind.String
            && string.Equals(status.GetString(), "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task<WaveSettings?> GetWaveSettingsAsync(IEnumerable<string>? seeds = null, CancellationToken cancellationToken = default)
    {
        var url = "/rotor/wave/settings";
        var joined = seeds is null ? string.Empty : string.Join(',', seeds.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (joined.Length > 0)
        {
            url += $"?seeds={Uri.EscapeDataString(joined)}";
        }

        return _connection.GetAsync<WaveSettings>(url, cancellationToken);
    }

    private static string FeedbackUrl(string station, string? batchId, string? streamId)
    {
        var query = new List<string>(2);
        if (!string.IsNullOrEmpty(batchId))
        {
            query.Add($"batch-id={Uri.EscapeDataString(batchId)}");
        }

        if (!string.IsNullOrEmpty(streamId))
        {
            query.Add($"streamId={Uri.EscapeDataString(streamId)}");
        }

        var url = $"/rotor/station/{Uri.EscapeDataString(station)}/feedback";
        return query.Count == 0 ? url : $"{url}?{string.Join('&', query)}";
    }
}
