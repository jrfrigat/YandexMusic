namespace YandexMusic;

/// <summary>
/// A structured error returned by the Yandex Music API in the <c>error</c> field of a response
/// envelope (for example <c>{ "name": "not-found", "message": "..." }</c>).
/// </summary>
public sealed class ApiError
{
    /// <summary>
    /// The short, machine-readable error code (for example <c>not-found</c>,
    /// <c>session-expired</c> or <c>validation</c>). May be empty if the API omits it.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The human-readable error description, when the API provides one; otherwise <see langword="null"/>.
    /// </summary>
    public string? Message { get; init; }
}
