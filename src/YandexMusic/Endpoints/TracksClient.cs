using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ITracksClient"/> implementation.</summary>
internal sealed class TracksClient : ITracksClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new tracks endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public TracksClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<Track?> GetAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);

        var tracks = await _connection
            .GetAsync<List<Track>>($"/tracks/{Uri.EscapeDataString(trackId)}", cancellationToken)
            .ConfigureAwait(false);

        return tracks is { Count: > 0 } ? tracks[0] : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Track>> GetManyAsync(IEnumerable<string> trackIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackIds);

        var ids = string.Join(',', trackIds);
        if (ids.Length == 0)
        {
            return [];
        }

        // The batch endpoint is a POST with the ids in the body (the single-track GET path-form does
        // not accept multiple ids).
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["track-ids"] = ids,
            ["with-positions"] = "True",
        });

        var tracks = await _connection
            .PostAsync<List<Track>>("/tracks", content, cancellationToken)
            .ConfigureAwait(false);

        return tracks ?? (IReadOnlyList<Track>)[];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DownloadInfo>> GetDownloadInfoAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);

        var infos = await _connection
            .GetAsync<List<DownloadInfo>>($"/tracks/{Uri.EscapeDataString(trackId)}/download-info", cancellationToken)
            .ConfigureAwait(false);

        return infos ?? (IReadOnlyList<DownloadInfo>)[];
    }

    /// <inheritdoc />
    public async Task<string?> GetDirectLinkAsync(string trackId, CancellationToken cancellationToken = default)
    {
        var variants = await GetDownloadInfoAsync(trackId, cancellationToken).ConfigureAwait(false);

        var variant = variants
            .Where(v => string.Equals(v.Codec, "mp3", StringComparison.OrdinalIgnoreCase) && !v.Preview && v.DownloadInfoUrl.Length > 0)
            .OrderByDescending(v => v.BitrateInKbps)
            .FirstOrDefault()
            ?? variants.FirstOrDefault(v => v.DownloadInfoUrl.Length > 0);

        if (variant is null)
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, variant.DownloadInfoUrl);
        using var response = await _connection
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var xml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return DownloadLinkResolver.BuildDirectLink(xml);
    }

    /// <inheritdoc />
    public Task<TrackSupplement?> GetSupplementAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return _connection.GetAsync<TrackSupplement>($"/tracks/{Uri.EscapeDataString(trackId)}/supplement", cancellationToken);
    }

    /// <inheritdoc />
    public Task<SimilarTracks?> GetSimilarAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return _connection.GetAsync<SimilarTracks>($"/tracks/{Uri.EscapeDataString(trackId)}/similar", cancellationToken);
    }

    /// <inheritdoc />
    public Task<TrackLyrics?> GetLyricsAsync(string trackId, string format = "TEXT", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);

        var signature = TrackRequestSigner.Sign(trackId);
        var url =
            $"/tracks/{Uri.EscapeDataString(trackId)}/lyrics?format={Uri.EscapeDataString(format)}&timeStamp={signature.Timestamp.ToString(CultureInfo.InvariantCulture)}&sign={Uri.EscapeDataString(signature.Value)}";
        return _connection.GetAsync<TrackLyrics>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TrackFullInfo?> GetFullInfoAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return _connection.GetAsync<TrackFullInfo>($"/tracks/{Uri.EscapeDataString(trackId)}/full-info", cancellationToken);
    }

    /// <inheritdoc />
    public Task<TrackTrailer?> GetTrailerAsync(string trackId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackId);
        return _connection.GetAsync<TrackTrailer>($"/tracks/{Uri.EscapeDataString(trackId)}/trailer", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> PlayAudioAsync(PlayAudioOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.TrackId);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.From);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.AlbumId);

        var now = FormatTimestamp(DateTimeOffset.UtcNow);
        var fields = new Dictionary<string, string>
        {
            ["track-id"] = options.TrackId,
            ["from-cache"] = options.FromCache ? "True" : "False",
            ["from"] = options.From,
            ["play-id"] = options.PlayId ?? string.Empty,
            ["timestamp"] = options.Timestamp is { } ts ? FormatTimestamp(ts) : now,
            ["track-length-seconds"] = options.TrackLengthSeconds.ToString(CultureInfo.InvariantCulture),
            ["total-played-seconds"] = options.TotalPlayedSeconds.ToString(CultureInfo.InvariantCulture),
            ["end-position-seconds"] = options.EndPositionSeconds.ToString(CultureInfo.InvariantCulture),
            ["album-id"] = options.AlbumId,
            ["client-now"] = options.ClientNow is { } clientNow ? FormatTimestamp(clientNow) : now,
        };

        if (options.Uid is { } uid)
        {
            fields["uid"] = uid.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrEmpty(options.PlaylistId))
        {
            fields["playlist-id"] = options.PlaylistId;
        }

        using var content = new FormUrlEncodedContent(fields);
        var result = await _connection
            .PostAsync<string>("/play-audio", content, cancellationToken)
            .ConfigureAwait(false);

        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<ShotEvent?> GetAfterTrackAsync(
        string nextTrackId,
        string contextItem,
        string? prevTrackId = null,
        string context = "playlist",
        string types = "shot",
        string from = "mobile-landing-origin-default",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nextTrackId);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextItem);

        var url =
            $"/after-track?from={Uri.EscapeDataString(from)}&nextTrackId={Uri.EscapeDataString(nextTrackId)}&context={Uri.EscapeDataString(context)}&contextItem={Uri.EscapeDataString(contextItem)}&types={Uri.EscapeDataString(types)}";
        if (!string.IsNullOrEmpty(prevTrackId))
        {
            url += $"&prevTrackId={Uri.EscapeDataString(prevTrackId)}";
        }

        var response = await _connection
            .GetAsync<AfterTrackResponse>(url, cancellationToken)
            .ConfigureAwait(false);

        return response?.ShotEvent;
    }

    // The play-audio endpoint expects an ISO-8601 timestamp with a trailing 'Z'.
    private static string FormatTimestamp(DateTimeOffset value)
        => value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "Z";
}
