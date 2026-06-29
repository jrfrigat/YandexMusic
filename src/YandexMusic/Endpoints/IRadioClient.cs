using YandexMusic.Models.Account;
using YandexMusic.Models.Radio;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the personalized radio (rotor) under <c>/rotor/*</c>. All require authentication.</summary>
public interface IRadioClient
{
    /// <summary>Gets the rotor account status, including radio-specific limits such as skips per hour.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The account status, or <see langword="null"/> when unavailable.</returns>
    Task<AccountStatus?> GetAccountStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the radio landing dashboard with its recommended stations.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The dashboard, or <see langword="null"/> when unavailable.</returns>
    Task<Dashboard?> GetStationsDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the full list of available stations.</summary>
    /// <param name="language">The catalogue language. Defaults to the client language when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The list of stations.</returns>
    Task<IReadOnlyList<StationResult>> GetStationsListAsync(string? language = null, CancellationToken cancellationToken = default);

    /// <summary>Sends a raw feedback event for a station.</summary>
    /// <param name="station">The station identity (for example <c>genre:pop</c>).</param>
    /// <param name="type">The feedback type, for example <c>radioStarted</c>, <c>trackStarted</c>, <c>trackFinished</c> or <c>skip</c>.</param>
    /// <param name="timestamp">The event time as unix epoch seconds. Defaults to now when omitted.</param>
    /// <param name="from">The source of the event, when applicable.</param>
    /// <param name="batchId">The batch identifier the event refers to, when applicable.</param>
    /// <param name="totalPlayedSeconds">The number of seconds played, when applicable.</param>
    /// <param name="trackId">The track the event refers to, when applicable.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendStationFeedbackAsync(
        string station,
        string type,
        double? timestamp = null,
        string? from = null,
        string? batchId = null,
        double? totalPlayedSeconds = null,
        string? trackId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that radio playback started for a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="from">The source of the event.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time as unix epoch seconds. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendStationFeedbackRadioStartedAsync(
        string station,
        string from,
        string? batchId = null,
        double? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track started playing on a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="trackId">The track that started.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time as unix epoch seconds. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendStationFeedbackTrackStartedAsync(
        string station,
        string trackId,
        string? batchId = null,
        double? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track finished playing on a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="trackId">The track that finished.</param>
    /// <param name="totalPlayedSeconds">The number of seconds played.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time as unix epoch seconds. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendStationFeedbackTrackFinishedAsync(
        string station,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        double? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track was skipped on a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="trackId">The track that was skipped.</param>
    /// <param name="totalPlayedSeconds">The number of seconds played before the skip.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time as unix epoch seconds. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendStationFeedbackSkipAsync(
        string station,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        double? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets detailed information about a single station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The station results.</returns>
    Task<IReadOnlyList<StationResult>> GetStationInfoAsync(string station, CancellationToken cancellationToken = default);

    /// <summary>Applies tuning settings to a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="moodEnergy">The mood/energy preference, for example <c>fun</c>, <c>active</c>, <c>calm</c>, <c>sad</c> or <c>all</c>.</param>
    /// <param name="diversity">The diversity preference, for example <c>favorite</c>, <c>popular</c>, <c>discover</c> or <c>default</c>.</param>
    /// <param name="type">The station type. Defaults to <c>rotor</c>; <c>generative</c> is also accepted.</param>
    /// <param name="language">The language preference. Defaults to <c>not-russian</c>; <c>russian</c> and <c>any</c> are also accepted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the settings were applied.</returns>
    Task<bool> SetStationSettingsAsync(
        string station,
        string moodEnergy,
        string diversity,
        string type = "rotor",
        string language = "not-russian",
        CancellationToken cancellationToken = default);

    /// <summary>Gets the next batch of tracks for a station.</summary>
    /// <param name="station">The station identity.</param>
    /// <param name="settings2">Whether to request the newer settings format. Official clients send <see langword="true"/>.</param>
    /// <param name="queue">The id of the just-played track to continue the chain from, when applicable.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The track batch, or <see langword="null"/> when unavailable.</returns>
    Task<StationTracksResult?> GetStationTracksAsync(
        string station,
        bool settings2 = true,
        string? queue = null,
        CancellationToken cancellationToken = default);
}
