using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Radio;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="IRadioSessionsClient"/> implementation.</summary>
internal sealed class RadioSessionsClient : IRadioSessionsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new radio sessions endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public RadioSessionsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<RotorSession?> CreateAsync(IEnumerable<string> seeds, RotorSessionOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        var seedList = seeds.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (seedList.Count == 0)
        {
            throw new ArgumentException("At least one seed is required.", nameof(seeds));
        }

        return PostSessionAsync("/rotor/session/new", seedList, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RotorSession?> CloneAsync(string radioSessionId, RotorSessionOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(radioSessionId);
        return PostSessionAsync($"{SessionPath(radioSessionId)}/clone", seeds: null, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RotorSessionTracks?> GetTracksAsync(
        string radioSessionId,
        IEnumerable<string>? queue = null,
        IEnumerable<SessionFeedback>? feedbacks = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(radioSessionId);
        var feedbackList = feedbacks?.ToList();

        // The queue is always sent, even empty: the server uses it to avoid repeats.
        using var content = JsonBody.FromObject(writer =>
        {
            WriteStrings(writer, "queue", queue ?? []);
            if (feedbackList is { Count: > 0 })
            {
                WriteFeedbackArray(writer, "feedbacks", feedbackList);
            }
        });

        return await _connection
            .PostAsync<RotorSessionTracks>($"{SessionPath(radioSessionId)}/tracks", content, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SendFeedbackAsync(string radioSessionId, SessionFeedback feedback, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(radioSessionId);
        ArgumentNullException.ThrowIfNull(feedback);

        using var content = JsonBody.FromObject(writer => WriteFeedbackMembers(writer, feedback));
        return await PostAcknowledgedAsync($"{SessionPath(radioSessionId)}/feedback", content, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<bool> SendRadioStartedAsync(
        string radioSessionId,
        string? batchId = null,
        string? from = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
        => SendEventAsync(radioSessionId, new SessionEvent { Type = "radioStarted", Timestamp = timestamp ?? DateTimeOffset.UtcNow }, batchId, from, cancellationToken);

    /// <inheritdoc />
    public Task<bool> SendTrackStartedAsync(
        string radioSessionId,
        string trackId,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        var @event = new SessionEvent { Type = "trackStarted", Timestamp = timestamp ?? DateTimeOffset.UtcNow, TrackId = trackId };
        return SendEventAsync(radioSessionId, @event, batchId, from: null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendTrackFinishedAsync(
        string radioSessionId,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        var @event = new SessionEvent
        {
            Type = "trackFinished",
            Timestamp = timestamp ?? DateTimeOffset.UtcNow,
            TrackId = trackId,
            TotalPlayedSeconds = totalPlayedSeconds,
        };
        return SendEventAsync(radioSessionId, @event, batchId, from: null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> SendSkipAsync(
        string radioSessionId,
        string trackId,
        double totalPlayedSeconds,
        string? batchId = null,
        DateTimeOffset? timestamp = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        var @event = new SessionEvent
        {
            Type = "skip",
            Timestamp = timestamp ?? DateTimeOffset.UtcNow,
            TrackId = trackId,
            TotalPlayedSeconds = totalPlayedSeconds,
        };
        return SendEventAsync(radioSessionId, @event, batchId, from: null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> SendFeedbacksAsync(string radioSessionId, IEnumerable<SessionFeedback> feedbacks, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(radioSessionId);
        ArgumentNullException.ThrowIfNull(feedbacks);
        var list = feedbacks.ToList();

        using var content = JsonBody.FromObject(writer => WriteFeedbackArray(writer, "feedbacks", list));
        return await PostAcknowledgedAsync($"{SessionPath(radioSessionId)}/feedbacks", content, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> SendSessionsFeedbacksAsync(IEnumerable<SessionFeedbacks> sessions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        var list = sessions.ToList();

        using var content = JsonBody.FromObject(writer =>
        {
            writer.WriteStartArray("sessions");
            foreach (var session in list)
            {
                writer.WriteStartObject();
                writer.WriteString("sessionId", session.SessionId);
                WriteFeedbackArray(writer, "feedbacks", session.Feedbacks);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        });

        return await PostAcknowledgedAsync("/rotor/sessions/feedbacks", content, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CombinedSession?> CreateCombinedAsync(
        IEnumerable<string>? supportedTypes = null,
        IEnumerable<CombinedSessionQueueItem>? queue = null,
        bool? child = null,
        bool? allowExplicit = null,
        CancellationToken cancellationToken = default)
    {
        var types = supportedTypes?.ToList();
        var items = queue?.ToList();

        using var content = JsonBody.FromObject(writer =>
        {
            if (types is not null)
            {
                WriteStrings(writer, "supportedTypes", types);
            }

            if (items is not null)
            {
                WriteQueueItems(writer, items);
            }

            WriteOptionalBoolean(writer, "child", child);
            WriteOptionalBoolean(writer, "allowExplicit", allowExplicit);
        });

        return await _connection
            .PostAsync<CombinedSession>("/rotor/combined/session/new", content, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CombinedSession?> GetCombinedNextAsync(
        string sessionId,
        IEnumerable<CombinedSessionQueueItem>? queue = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        var items = queue?.ToList() ?? [];

        using var content = JsonBody.FromObject(writer => WriteQueueItems(writer, items));
        return await _connection
            .PostAsync<CombinedSession>($"/rotor/combined/session/{Uri.EscapeDataString(sessionId)}/next", content, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CombinedSessionLanding?> GetCombinedLandingAsync(
        IEnumerable<string>? supportedTypes = null,
        bool? child = null,
        bool? allowExplicit = null,
        CancellationToken cancellationToken = default)
    {
        var types = supportedTypes?.ToList();

        using var content = JsonBody.FromObject(writer =>
        {
            if (types is not null)
            {
                WriteStrings(writer, "supportedTypes", types);
            }

            WriteOptionalBoolean(writer, "child", child);
            WriteOptionalBoolean(writer, "allowExplicit", allowExplicit);
        });

        return await _connection
            .PostAsync<CombinedSessionLanding>("/rotor/combined/session/landing", content, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<RotorSession?> PostSessionAsync(string url, IReadOnlyList<string>? seeds, RotorSessionOptions? options, CancellationToken cancellationToken)
    {
        using var content = JsonBody.FromObject(writer =>
        {
            if (seeds is not null)
            {
                WriteStrings(writer, "seeds", seeds);
            }

            if (options is null)
            {
                return;
            }

            if (options.Queue is not null)
            {
                WriteStrings(writer, "queue", options.Queue);
            }

            if (!string.IsNullOrEmpty(options.TrackToStartFrom))
            {
                writer.WriteString("trackToStartFrom", options.TrackToStartFrom);
            }

            WriteOptionalBoolean(writer, "includeTracksInResponse", options.IncludeTracksInResponse);
            WriteOptionalBoolean(writer, "includeWaveModel", options.IncludeWaveModel);
            WriteOptionalBoolean(writer, "interactive", options.Interactive);
            WriteOptionalBoolean(writer, "incognito", options.Incognito);
            WriteOptionalBoolean(writer, "child", options.Child);
            WriteOptionalBoolean(writer, "allowExplicit", options.AllowExplicit);
        });

        return await _connection.PostAsync<RotorSession>(url, content, cancellationToken).ConfigureAwait(false);
    }

    private Task<bool> SendEventAsync(string radioSessionId, SessionEvent @event, string? batchId, string? from, CancellationToken cancellationToken)
        => SendFeedbackAsync(radioSessionId, new SessionFeedback { Event = @event, BatchId = batchId, From = from }, cancellationToken);

    // Session feedback endpoints acknowledge with an object payload rather than the literal "ok".
    private async Task<bool> PostAcknowledgedAsync(string url, HttpContent content, CancellationToken cancellationToken)
    {
        var result = await _connection.PostAsync<JsonElement>(url, content, cancellationToken).ConfigureAwait(false);
        return result.ValueKind == JsonValueKind.Object;
    }

    private static string SessionPath(string radioSessionId) => $"/rotor/session/{Uri.EscapeDataString(radioSessionId)}";

    private static void WriteStrings(Utf8JsonWriter writer, string name, IEnumerable<string> values)
    {
        writer.WriteStartArray(name);
        foreach (var value in values)
        {
            writer.WriteStringValue(value);
        }

        writer.WriteEndArray();
    }

    private static void WriteOptionalBoolean(Utf8JsonWriter writer, string name, bool? value)
    {
        if (value is { } flag)
        {
            writer.WriteBoolean(name, flag);
        }
    }

    private static void WriteQueueItems(Utf8JsonWriter writer, IEnumerable<CombinedSessionQueueItem> items)
    {
        writer.WriteStartArray("queue");
        foreach (var item in items)
        {
            writer.WriteStartObject();
            writer.WriteString("type", item.Type);
            writer.WriteString("id", item.Id);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteFeedbackArray(Utf8JsonWriter writer, string name, IEnumerable<SessionFeedback> feedbacks)
    {
        writer.WriteStartArray(name);
        foreach (var feedback in feedbacks)
        {
            writer.WriteStartObject();
            WriteFeedbackMembers(writer, feedback);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteFeedbackMembers(Utf8JsonWriter writer, SessionFeedback feedback)
    {
        var @event = feedback.Event;
        writer.WriteStartObject("event");
        writer.WriteString("type", @event.Type);
        writer.WriteString("timestamp", JsonBody.FormatTimestamp(@event.Timestamp));

        if (!string.IsNullOrEmpty(@event.TrackId))
        {
            writer.WriteString("trackId", @event.TrackId);
        }

        if (@event.TotalPlayedSeconds is { } played)
        {
            writer.WriteNumber("totalPlayedSeconds", played);
        }

        if (@event.Playable is { } playable)
        {
            writer.WriteStartObject("playable");
            writer.WriteString("type", playable.Type);
            if (!string.IsNullOrEmpty(playable.TrackId))
            {
                writer.WriteString("trackId", playable.TrackId);
            }

            if (!string.IsNullOrEmpty(playable.Id))
            {
                writer.WriteString("id", playable.Id);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();

        if (!string.IsNullOrEmpty(feedback.BatchId))
        {
            writer.WriteString("batchId", feedback.BatchId);
        }

        if (!string.IsNullOrEmpty(feedback.From))
        {
            writer.WriteString("from", feedback.From);
        }
    }
}
