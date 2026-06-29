using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models;
using YandexMusic.Models.Labels;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ILabelsClient"/> implementation.</summary>
internal sealed class LabelsClient : ILabelsClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new labels endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public LabelsClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<Label?> GetAsync(string labelId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labelId);
        return _connection.GetAsync<Label>($"/labels/{Uri.EscapeDataString(labelId)}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<LabelAlbums?> GetAlbumsAsync(
        string labelId,
        int page = 0,
        int pageSize = 100,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labelId);

        var url =
            $"/labels/{Uri.EscapeDataString(labelId)}/albums?page={page.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        if (!string.IsNullOrEmpty(sortBy))
        {
            url += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        }

        if (!string.IsNullOrEmpty(sortOrder))
        {
            url += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
        }

        return _connection.GetAsync<LabelAlbums>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LabelArtists?> GetArtistsAsync(
        string labelId,
        int page = 0,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(labelId);

        var url =
            $"/labels/{Uri.EscapeDataString(labelId)}/artists?page={page.ToString(CultureInfo.InvariantCulture)}&pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}";
        return _connection.GetAsync<LabelArtists>(url, cancellationToken);
    }
}
