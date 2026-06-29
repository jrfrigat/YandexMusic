namespace YandexMusic;

/// <summary>
/// Thrown when a successful HTTP response cannot be deserialized into the expected model — for
/// example when the API returns a payload whose shape differs from what the client expects.
/// </summary>
public sealed class YandexMusicSerializationException : YandexMusicException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YandexMusicSerializationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the serialization failure.</param>
    /// <param name="innerException">The underlying exception (typically a <see cref="System.Text.Json.JsonException"/>).</param>
    public YandexMusicSerializationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
