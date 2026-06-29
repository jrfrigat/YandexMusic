namespace YandexMusic;

/// <summary>
/// Thrown when authentication fails — for example an invalid or expired token, a rejected
/// captcha, or a sign-in flow that did not complete successfully.
/// </summary>
public sealed class YandexMusicAuthenticationException : YandexMusicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicAuthenticationException"/> class
    /// with the specified error message.
    /// </summary>
    /// <param name="message">The message that describes the authentication failure.</param>
    public YandexMusicAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicAuthenticationException"/> class
    /// with the specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the authentication failure.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public YandexMusicAuthenticationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
