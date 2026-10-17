namespace YandexMusicTerminal.Catalog;

/// <summary>Stream URLs in the preferred quality, with the standard MP3 as the fallback.</summary>
public sealed partial class MusicCatalog
{
    // Codecs Media Foundation opens from a URL. "flac-mp4" is left out: FLAC inside MP4 is not
    // something every Windows build can decode, and the server would happily pick it.
    private static readonly string[] LosslessCodecs = ["flac", "aac-mp4", "aac", "mp3"];

    /// <inheritdoc />
    public bool PreferLossless { get; set; }

    /// <inheritdoc />
    public async Task<string?> ResolveStreamUrlAsync(string trackId, CancellationToken cancellationToken = default)
    {
        if (!PreferLossless)
        {
            return await ResolveFallbackStreamUrlAsync(trackId, cancellationToken).ConfigureAwait(false);
        }

        // "raw" keeps the file plain so the player can stream the URL as is; "encraw" would need the
        // whole file downloaded and decrypted first.
        var info = await _client.Tracks
            .GetFileInfoAsync(trackId, "lossless", LosslessCodecs, "raw", cancellationToken)
            .ConfigureAwait(false);

        return info?.DownloadInfo is { Url.Length: > 0, IsEncrypted: false } file
            ? file.Url
            : await ResolveFallbackStreamUrlAsync(trackId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<string?> ResolveFallbackStreamUrlAsync(string trackId, CancellationToken cancellationToken = default)
        => _client.Tracks.GetDirectLinkAsync(trackId, cancellationToken);
}
