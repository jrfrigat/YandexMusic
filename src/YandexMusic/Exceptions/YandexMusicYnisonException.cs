namespace YandexMusic.Exceptions;

/// <summary>A failure of the Ynison real-time session: the handshake, a frame, or the connection lifecycle.</summary>
public sealed class YandexMusicYnisonException : YandexMusicException
{
    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The failure description.</param>
    public YandexMusicYnisonException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and a cause.</summary>
    /// <param name="message">The failure description.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public YandexMusicYnisonException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
