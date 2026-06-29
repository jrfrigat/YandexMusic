namespace YandexMusic;

/// <summary>
/// The base class for every exception thrown by the YandexMusic client. Catch this type to handle
/// any error originating from the library.
/// </summary>
public class YandexMusicException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="YandexMusicException"/> class.</summary>
    public YandexMusicException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicException"/> class with the
    /// specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public YandexMusicException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicException"/> class with the
    /// specified error message and a reference to the inner exception that caused it.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public YandexMusicException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
