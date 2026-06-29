using System.Globalization;
using YandexMusic.Http;
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
        double? timestamp = null,
        string? from = null,
        string? batchId = null,
        double? totalPlayedSeconds = null,
        string? trackId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        var fields = new Dictionary<string, string>
        {
            ["type"] = type,
            ["timestamp"] = (timestamp ?? CurrentUnixSeconds()).ToString(CultureInfo.InvariantCulture),
        };

        if (!string.IsNullOrEmpty(from))
        {
            fields["from"] = from;
        }

        if (!string.IsNullOrEmpty(trackId))
        {
            fields["trackId"] = trackId;
        }

        if (totalPlayedSeconds is { } played)
        {
            fields["totalPlayedSeconds"] = played.ToString(CultureInfo.InvariantCulture);
        }

        var url = $"/rotor/station/{Uri.EscapeDataString(station)}/feedback";
        if (!string.IsNullOrEmpty(batchId))
        {
            url += $"?batch-id={Uri.EscapeDataString(batchId)}";
        }

        using var content = new FormUrlEncodedContent(fields);
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
        double? timestamp = null,
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
        double? timestamp = null,
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
        double? timestamp = null,
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
        double? timestamp = null,
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

        var fields = new Dictionary<string, string>
        {
            ["moodEnergy"] = moodEnergy,
            ["diversity"] = diversity,
            ["type"] = type,
            ["language"] = language,
        };

        using var content = new FormUrlEncodedContent(fields);
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

    // The rotor feedback endpoint expects timestamps as unix epoch seconds.
    private static double CurrentUnixSeconds()
        => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
}
