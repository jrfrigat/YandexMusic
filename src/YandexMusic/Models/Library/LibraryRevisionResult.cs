namespace YandexMusic.Models.Library;

/// <summary>
/// The result of a track like/dislike mutation. The revision is present on success and absent on failure.
/// </summary>
internal sealed class LibraryRevisionResult
{
    /// <summary>The library revision after the mutation, when the request succeeded.</summary>
    public int? Revision { get; init; }
}
