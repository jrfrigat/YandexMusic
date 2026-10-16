namespace YandexMusic.Exceptions;

/// <summary>What a Ynison server error means for the connection.</summary>
public enum YnisonServerErrorKind
{
    /// <summary>A rejected request or a server-side failure; the session can continue or reconnect.</summary>
    Other = 0,

    /// <summary>The token was rejected (invalid or expired). Reconnecting with the same token cannot help.</summary>
    Unauthorized = 1,

    /// <summary>
    /// Another connection with the same device identifier displaced this one. A device identifier
    /// can back only one live connection, so reconnecting would just displace the other client.
    /// </summary>
    DeviceDisplaced = 2,
}

/// <summary>
/// An error the Ynison server reported in an <c>error</c> frame or at the handshake. Besides the
/// message it carries the server's advice on retrying: the backoff ladder it wants reconnects to
/// follow and, when set, how long it asks clients to stay away.
/// </summary>
public sealed class YandexMusicYnisonServerException : YandexMusicYnisonException
{
    /// <summary>Creates the exception from the details of a server error.</summary>
    /// <param name="message">The server's error text.</param>
    /// <param name="kind">What the error means for the connection.</param>
    /// <param name="grpcCode">The gRPC status code, when reported.</param>
    /// <param name="httpCode">The HTTP status code, when reported.</param>
    /// <param name="errorCode">The internal Ynison error code (<c>ynison-error-code</c>), when reported.</param>
    /// <param name="backoff">The reconnect delays the server recommends, in order.</param>
    /// <param name="goAway">How long the server asks clients not to reconnect, when set.</param>
    public YandexMusicYnisonServerException(
        string message,
        YnisonServerErrorKind kind = YnisonServerErrorKind.Other,
        int? grpcCode = null,
        int? httpCode = null,
        string? errorCode = null,
        IReadOnlyList<TimeSpan>? backoff = null,
        TimeSpan? goAway = null)
        : base(message)
    {
        Kind = kind;
        GrpcCode = grpcCode;
        HttpCode = httpCode;
        ErrorCode = errorCode;
        Backoff = backoff ?? [];
        GoAway = goAway;
    }

    /// <summary>What the error means for the connection.</summary>
    public YnisonServerErrorKind Kind { get; }

    /// <summary>The gRPC status code, when reported.</summary>
    public int? GrpcCode { get; }

    /// <summary>The HTTP status code, when reported.</summary>
    public int? HttpCode { get; }

    /// <summary>The internal Ynison error code (<c>ynison-error-code</c>), when reported.</summary>
    public string? ErrorCode { get; }

    /// <summary>The reconnect delays the server recommends, in order; empty when it gave none.</summary>
    public IReadOnlyList<TimeSpan> Backoff { get; }

    /// <summary>How long the server asks clients not to reconnect, when set.</summary>
    public TimeSpan? GoAway { get; }

    /// <summary>
    /// Whether reconnecting is pointless: the token was rejected, the device was displaced, or the
    /// server asked clients to stay away.
    /// </summary>
    public bool IsTerminal => Kind != YnisonServerErrorKind.Other || GoAway is { } goAway && goAway > TimeSpan.Zero;
}
