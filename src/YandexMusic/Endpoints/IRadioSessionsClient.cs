using YandexMusic.Models.Radio;

namespace YandexMusic.Endpoints;

/// <summary>
/// Endpoints for radio sessions under <c>/rotor/session*</c> and <c>/rotor/combined/session*</c>.
/// A session is the current way to play radio: it is opened from seeds, serves batches of tracks
/// and takes playback feedback. All require authentication.
/// </summary>
/// <remarks>
/// The typical loop is: <see cref="CreateAsync"/>, report <c>radioStarted</c> and
/// <c>trackStarted</c>, report <c>trackFinished</c> or <c>skip</c> for each track, and call
/// <see cref="GetTracksAsync"/> with the tracks already received when the batch runs low. A session
/// can be started already tuned by passing wave setting seeds such as
/// <c>settingDiversity:discover</c> (see <see cref="EnumValue.SerializedSeed"/>).
/// </remarks>
public interface IRadioSessionsClient
{
    /// <summary>Opens a new radio session.</summary>
    /// <param name="seeds">The session seeds, for example <c>user:onyourwave</c>, <c>genre:rock</c> or <c>track:12345</c>.</param>
    /// <param name="options">Optional session parameters.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The session, or <see langword="null"/> when the API returns none.</returns>
    Task<RotorSession?> CreateAsync(IEnumerable<string> seeds, RotorSessionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Opens a new radio session with the same seeds as an existing one.</summary>
    /// <param name="radioSessionId">The session to copy.</param>
    /// <param name="options">Optional session parameters.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The new session, or <see langword="null"/> when the API returns none.</returns>
    Task<RotorSession?> CloneAsync(string radioSessionId, RotorSessionOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next batch of tracks of a session. An unknown session is not an error: the result
    /// has <see cref="RotorSessionTracks.UnknownSession"/> set, and a new session must be opened.
    /// </summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="queue">Tracks already received (as <c>id</c> or <c>id:albumId</c>), so they are not served again.</param>
    /// <param name="feedbacks">Feedback to report in the same request, if any.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The batch, or <see langword="null"/> when the API returns none.</returns>
    Task<RotorSessionTracks?> GetTracksAsync(
        string radioSessionId,
        IEnumerable<string>? queue = null,
        IEnumerable<SessionFeedback>? feedbacks = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports one feedback event of a session.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="feedback">The event with its batch and source.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendFeedbackAsync(string radioSessionId, SessionFeedback feedback, CancellationToken cancellationToken = default);

    /// <summary>Reports that playback of a session started.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="from">Where playback was started from, when applicable.</param>
    /// <param name="timestamp">The event time. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendRadioStartedAsync(
        string radioSessionId,
        string? batchId = null,
        string? from = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track of a session started playing.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="trackId">The track, as <c>id</c> or <c>id:albumId</c>.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendTrackStartedAsync(
        string radioSessionId,
        string trackId,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track of a session was played to the end.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="trackId">The track, as <c>id</c> or <c>id:albumId</c>.</param>
    /// <param name="totalPlayedSeconds">How many seconds of the track were played.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendTrackFinishedAsync(
        string radioSessionId,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports that a track of a session was skipped.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="trackId">The track, as <c>id</c> or <c>id:albumId</c>.</param>
    /// <param name="totalPlayedSeconds">How many seconds of the track were played before the skip.</param>
    /// <param name="batchId">The batch identifier, when applicable.</param>
    /// <param name="timestamp">The event time. Defaults to now when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the event.</returns>
    Task<bool> SendSkipAsync(
        string radioSessionId,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reports several feedback events of one session in a single request.</summary>
    /// <param name="radioSessionId">The session identifier.</param>
    /// <param name="feedbacks">The events to report.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the events.</returns>
    Task<bool> SendFeedbacksAsync(string radioSessionId, IEnumerable<SessionFeedback> feedbacks, CancellationToken cancellationToken = default);

    /// <summary>Reports feedback for several sessions in a single request, for example after playing offline.</summary>
    /// <param name="sessions">The events grouped by session.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API acknowledged the events.</returns>
    Task<bool> SendSessionsFeedbacksAsync(IEnumerable<SessionFeedbacks> sessions, CancellationToken cancellationToken = default);

    /// <summary>Opens a combined session, which mixes tracks and clips.</summary>
    /// <param name="supportedTypes">The item types the caller can play. Known values: <c>CLIP</c>, <c>TRACK</c>.</param>
    /// <param name="queue">Items already received.</param>
    /// <param name="child">Whether the child mode is on.</param>
    /// <param name="allowExplicit">Whether explicit content is allowed.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The session with its first items, or <see langword="null"/> when the API returns none.</returns>
    Task<CombinedSession?> CreateCombinedAsync(
        IEnumerable<string>? supportedTypes = null,
        IEnumerable<CombinedSessionQueueItem>? queue = null,
        bool? child = null,
        bool? allowExplicit = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the next items of a combined session.</summary>
    /// <param name="sessionId">The combined session identifier (<see cref="CombinedSession.SessionId"/>).</param>
    /// <param name="queue">Items already received.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The next items, or <see langword="null"/> when the API returns none.</returns>
    Task<CombinedSession?> GetCombinedNextAsync(
        string sessionId,
        IEnumerable<CombinedSessionQueueItem>? queue = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the landing block of the combined session, for example the "clip time" promo.</summary>
    /// <param name="supportedTypes">The item types the caller can play. Known values: <c>CLIP</c>, <c>TRACK</c>.</param>
    /// <param name="child">Whether the child mode is on.</param>
    /// <param name="allowExplicit">Whether explicit content is allowed.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The landing block, or <see langword="null"/> when the API returns none.</returns>
    Task<CombinedSessionLanding?> GetCombinedLandingAsync(
        IEnumerable<string>? supportedTypes = null,
        bool? child = null,
        bool? allowExplicit = null,
        CancellationToken cancellationToken = default);
}
