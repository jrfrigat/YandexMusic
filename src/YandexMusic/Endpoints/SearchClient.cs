using System.Globalization;
using YandexMusic.Http;
using YandexMusic.Models.Search;

namespace YandexMusic.Endpoints;

/// <summary>The default <see cref="ISearchClient"/> implementation.</summary>
internal sealed class SearchClient : ISearchClient
{
    private readonly IYandexMusicConnection _connection;

    /// <summary>Initializes a new search endpoint over the shared connection.</summary>
    /// <param name="connection">The request engine.</param>
    public SearchClient(IYandexMusicConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public Task<SearchResult?> SearchAsync(
        string text,
        SearchType type = SearchType.All,
        int page = 0,
        bool correctMisspells = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var nocorrect = correctMisspells ? "false" : "true";
        var pageValue = page.ToString(CultureInfo.InvariantCulture);
        var url = $"/search?text={Uri.EscapeDataString(text)}&type={ToParameter(type)}&page={pageValue}&nocorrect={nocorrect}";
        return _connection.GetAsync<SearchResult>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<SearchSuggestions?> SuggestAsync(string part, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(part);
        return _connection.GetAsync<SearchSuggestions>($"/search/suggest?part={Uri.EscapeDataString(part)}", cancellationToken);
    }

    private static string ToParameter(SearchType type) => type switch
    {
        SearchType.All => "all",
        SearchType.Track => "track",
        SearchType.Album => "album",
        SearchType.Artist => "artist",
        SearchType.Playlist => "playlist",
        SearchType.Podcast => "podcast",
        SearchType.PodcastEpisode => "podcast_episode",
        SearchType.Video => "video",
        SearchType.User => "user",
        _ => "all",
    };
}
