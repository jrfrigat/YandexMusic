using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace YandexMusicTerminal.Diagnostics;

/// <summary>
/// Records every API call into the <see cref="RequestLog"/>: the request line and headers, the
/// status and timing, and as much of each body as is useful to read. It sits in front of the
/// library's own handler, so it sees requests exactly as they go out and responses exactly as they
/// come back. While the journal is off it does nothing but forward the call.
/// </summary>
public sealed class LoggingHttpHandler : DelegatingHandler
{
    private const int MaxBodyCharacters = 8 * 1024;

    private readonly RequestLog _log;

    /// <summary>Creates the handler.</summary>
    /// <param name="log">The journal to write to.</param>
    public LoggingHttpHandler(RequestLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        _log = log;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!_log.IsEnabled)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var builder = new StringBuilder()
            .Append("--> ").Append(request.Method).Append(' ').AppendLine(request.RequestUri?.ToString());
        AppendHeaders(builder, request.Headers);
        if (request.Content is not null)
        {
            AppendHeaders(builder, request.Content.Headers);
            builder.AppendLine(await ReadBodyAsync(request.Content, cancellationToken).ConfigureAwait(false));
        }

        _log.Write("http", builder.ToString().TrimEnd());

        var clock = Stopwatch.StartNew();
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            clock.Stop();
            _log.Write("http", $"<-- FAILED after {clock.ElapsedMilliseconds} ms: {ex.GetType().Name}: {ex.Message}");
            throw;
        }

        clock.Stop();
        var answer = new StringBuilder()
            .Append("<-- ").Append((int)response.StatusCode).Append(' ').Append(response.ReasonPhrase)
            .Append(" (").Append(clock.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)).AppendLine(" ms)");
        AppendHeaders(answer, response.Headers);
        AppendHeaders(answer, response.Content.Headers);
        answer.AppendLine(await ReadBodyAsync(response.Content, cancellationToken).ConfigureAwait(false));

        _log.Write("http", answer.ToString().TrimEnd());
        return response;
    }

    private static void AppendHeaders(StringBuilder builder, HttpHeaders headers)
    {
        foreach (var (name, values) in headers)
        {
            _ = builder.Append("    ").Append(name).Append(": ").AppendLine(string.Join(", ", values));
        }
    }

    /// <summary>
    /// Reads a body for the journal without consuming it: the content is buffered first, so the
    /// caller downstream still gets to read the same stream.
    /// </summary>
    private static async Task<string> ReadBodyAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var mediaType = content.Headers.ContentType?.MediaType ?? string.Empty;
        var isText = mediaType.Contains("json", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("text", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)
            || mediaType.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
        if (!isText)
        {
            return $"    ({(mediaType.Length == 0 ? "no" : mediaType)} body, not logged)";
        }

        try
        {
            await content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
            var body = await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return body.Length > MaxBodyCharacters
                ? "    " + body[..MaxBodyCharacters] + $"… ({body.Length} chars total)"
                : "    " + body;
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException or InvalidOperationException)
        {
            return $"    (body unavailable: {ex.GetType().Name})";
        }
    }
}
