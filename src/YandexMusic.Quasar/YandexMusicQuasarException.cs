namespace YandexMusic.Exceptions;

/// <summary>
/// A failure of local device support: the Quasar backend, the connection to a speaker, or a command
/// the speaker refused.
/// </summary>
public sealed class YandexMusicQuasarException : YandexMusicException
{
    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The failure description.</param>
    public YandexMusicQuasarException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and a cause.</summary>
    /// <param name="message">The failure description.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public YandexMusicQuasarException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
