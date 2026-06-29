using YandexMusic.Authentication;
using YandexMusic.Endpoints;
using YandexMusic.Http;

namespace YandexMusic;

/// <summary>
/// The default <see cref="IYandexMusicClient"/> implementation. It owns an <see cref="HttpClient"/>
/// (unless one is supplied), an <see cref="AuthSession"/> and the shared request engine, and
/// exposes them through the typed endpoint groups.
/// </summary>
public sealed class YandexMusicClient : IYandexMusicClient
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private int _disposed;

    /// <summary>Creates a client with default options.</summary>
    public YandexMusicClient()
        : this(new YandexMusicClientOptions())
    {
    }

    /// <summary>Creates a client configured by <paramref name="options"/>.</summary>
    /// <param name="options">The client options.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public YandexMusicClient(YandexMusicClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var session = new AuthSession(options.DeviceId);
        _httpClient = YandexMusicHttpClientFactory.Create(options, session);
        _ownsHttpClient = true;

        Connection = new YandexMusicConnection(_httpClient, session);
        Authentication = new AuthenticationClient(session);
        Tracks = new TracksClient(Connection);
        Search = new SearchClient(Connection);
        Albums = new AlbumsClient(Connection);
        Artists = new ArtistsClient(Connection);
        Playlists = new PlaylistsClient(Connection);
        Account = new AccountClient(Connection);
        Library = new LibraryClient(Connection);
        Genres = new GenresClient(Connection);
        Clips = new ClipsClient(Connection);
        Credits = new CreditsClient(Connection);
        Disclaimers = new DisclaimersClient(Connection);
        Labels = new LabelsClient(Connection);
        Landing = new LandingClient(Connection);
        Radio = new RadioClient(Connection);
        Concerts = new ConcertsClient(Connection);
        Metatags = new MetatagsClient(Connection);
        Queue = new QueueClient(Connection);
        Pins = new PinsClient(Connection);
        Presaves = new PresavesClient(Connection);
        MusicHistory = new MusicHistoryClient(Connection);
    }

    /// <summary>
    /// Creates a client over an externally-owned <see cref="HttpClient"/> and
    /// <see cref="AuthSession"/>. The client does not dispose the supplied <paramref name="httpClient"/>.
    /// Intended for dependency-injection scenarios where the handler is pooled.
    /// </summary>
    /// <param name="httpClient">A configured HTTP client whose handler shares <paramref name="session"/>'s cookie container.</param>
    /// <param name="session">The authentication session to use.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null"/>.</exception>
    public YandexMusicClient(HttpClient httpClient, AuthSession session)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(session);

        _httpClient = httpClient;
        _ownsHttpClient = false;

        Connection = new YandexMusicConnection(_httpClient, session);
        Authentication = new AuthenticationClient(session);
        Tracks = new TracksClient(Connection);
        Search = new SearchClient(Connection);
        Albums = new AlbumsClient(Connection);
        Artists = new ArtistsClient(Connection);
        Playlists = new PlaylistsClient(Connection);
        Account = new AccountClient(Connection);
        Library = new LibraryClient(Connection);
        Genres = new GenresClient(Connection);
        Clips = new ClipsClient(Connection);
        Credits = new CreditsClient(Connection);
        Disclaimers = new DisclaimersClient(Connection);
        Labels = new LabelsClient(Connection);
        Landing = new LandingClient(Connection);
        Radio = new RadioClient(Connection);
        Concerts = new ConcertsClient(Connection);
        Metatags = new MetatagsClient(Connection);
        Queue = new QueueClient(Connection);
        Pins = new PinsClient(Connection);
        Presaves = new PresavesClient(Connection);
        MusicHistory = new MusicHistoryClient(Connection);
    }

    /// <inheritdoc />
    public IAuthenticationClient Authentication { get; }

    /// <inheritdoc />
    public ITracksClient Tracks { get; }

    /// <inheritdoc />
    public ISearchClient Search { get; }

    /// <inheritdoc />
    public IAlbumsClient Albums { get; }

    /// <inheritdoc />
    public IArtistsClient Artists { get; }

    /// <inheritdoc />
    public IPlaylistsClient Playlists { get; }

    /// <inheritdoc />
    public IAccountClient Account { get; }

    /// <inheritdoc />
    public ILibraryClient Library { get; }

    /// <inheritdoc />
    public IGenresClient Genres { get; }

    /// <inheritdoc />
    public IClipsClient Clips { get; }

    /// <inheritdoc />
    public ICreditsClient Credits { get; }

    /// <inheritdoc />
    public IDisclaimersClient Disclaimers { get; }

    /// <inheritdoc />
    public ILabelsClient Labels { get; }

    /// <inheritdoc />
    public ILandingClient Landing { get; }

    /// <inheritdoc />
    public IRadioClient Radio { get; }

    /// <inheritdoc />
    public IConcertsClient Concerts { get; }

    /// <inheritdoc />
    public IMetatagsClient Metatags { get; }

    /// <inheritdoc />
    public IQueueClient Queue { get; }

    /// <inheritdoc />
    public IPinsClient Pins { get; }

    /// <inheritdoc />
    public IPresavesClient Presaves { get; }

    /// <inheritdoc />
    public IMusicHistoryClient MusicHistory { get; }

    /// <summary>The shared request engine used by the endpoint groups.</summary>
    internal IYandexMusicConnection Connection { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        // Guard against concurrent/double disposal of the owned handler.
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
