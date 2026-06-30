// A small console sample for the YandexMusic library. It demonstrates the most common read
// operations — tracks, albums, artists, search, charts, new releases and a user's library.
//
//   dotnet run --project samples/YandexMusic.Cli -- <command> [argument]
//
// A token is only needed for personal data (the `liked` command) and for media links/lyrics.
// Provide it via the YANDEX_MUSIC_TOKEN environment variable.
using System.Globalization;
using YandexMusic;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Tracks;

var token = Environment.GetEnvironmentVariable("YANDEX_MUSIC_TOKEN");

await using var client = new YandexMusicClient();
if (!string.IsNullOrWhiteSpace(token))
{
    client.Authentication.SignInWithToken(token);
}

var command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";
var value = args.Length > 1 ? args[1] : null;

try
{
    switch (command)
    {
        case "track": await ShowTrackAsync(client, value ?? "4"); break;
        case "tracks": await ShowTracksAsync(client, value ?? "4,5,6"); break;
        case "album": await ShowAlbumAsync(client, value ?? "3"); break;
        case "artist": await ShowArtistAsync(client, value ?? "79215"); break;
        case "search": await ShowSearchAsync(client, value ?? "Queen"); break;
        case "new-releases": await ShowNewReleasesAsync(client); break;
        case "chart": await ShowChartAsync(client, value ?? "russia"); break;
        case "link": await ShowLinkAsync(client, value ?? "4"); break;
        case "liked": await ShowLikedAsync(client, token); break;
        default: PrintHelp(); break;
    }

    return 0;
}
catch (YandexMusicException ex)
{
    Console.Error.WriteLine($"API error: {ex.Message}");
    if (ex.InnerException is { } inner)
    {
        Console.Error.WriteLine($"  ↳ {inner.Message}");
    }

    return 1;
}

static async Task ShowTrackAsync(IYandexMusicClient client, string id)
{
    var track = await client.Tracks.GetAsync(id);
    if (track is null)
    {
        Console.WriteLine($"Track {id} not found.");
        return;
    }

    Console.WriteLine($"Track: {track.Title} — {FormatArtists(track.Artists)} ({FormatDuration(track.DurationMs)})");

    var similar = await client.Tracks.GetSimilarAsync(id);
    if (similar?.Tracks is { Count: > 0 } tracks)
    {
        Console.WriteLine("\nSimilar:");
        foreach (var t in tracks.Take(5))
        {
            Console.WriteLine($"  - {t.Title} — {FormatArtists(t.Artists)}");
        }
    }
}

static async Task ShowTracksAsync(IYandexMusicClient client, string ids)
{
    var tracks = await client.Tracks.GetManyAsync(ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    Console.WriteLine($"Fetched {tracks.Count} track(s):");
    foreach (var t in tracks)
    {
        Console.WriteLine($"  {t.Id,-12} {t.Title} — {FormatArtists(t.Artists)}");
    }
}

static async Task ShowAlbumAsync(IYandexMusicClient client, string id)
{
    var album = await client.Albums.GetWithTracksAsync(id);
    if (album is null)
    {
        Console.WriteLine($"Album {id} not found.");
        return;
    }

    Console.WriteLine($"Album: {album.Title} — {FormatArtists(album.Artists)}");
    Console.WriteLine($"Year: {album.Year?.ToString(CultureInfo.InvariantCulture) ?? "—"}   Genre: {album.Genre ?? "—"}   Tracks: {album.TrackCount}");

    if (album.Volumes is { Count: > 0 })
    {
        Console.WriteLine("\nTracklist:");
        var n = 1;
        foreach (var disc in album.Volumes)
        {
            foreach (var t in disc)
            {
                Console.WriteLine($"  {n++,3}. {t.Title} ({FormatDuration(t.DurationMs)})");
            }
        }
    }
}

static async Task ShowArtistAsync(IYandexMusicClient client, string id)
{
    var info = await client.Artists.GetBriefInfoAsync(id);
    if (info is null)
    {
        Console.WriteLine($"Artist {id} not found.");
        return;
    }

    Console.WriteLine($"Artist: {info.Artist.Name}");
    if (info.Artist.Genres is { Count: > 0 } genres)
    {
        Console.WriteLine($"Genres: {string.Join(", ", genres)}");
    }

    Console.WriteLine("\nPopular tracks:");
    foreach (var t in info.PopularTracks.Take(5))
    {
        Console.WriteLine($"  - {t.Title} ({FormatDuration(t.DurationMs)})");
    }

    Console.WriteLine("\nAlbums:");
    foreach (var a in info.Albums.Take(5))
    {
        Console.WriteLine($"  - {a.Title} ({a.Year?.ToString(CultureInfo.InvariantCulture) ?? "—"})");
    }
}

static async Task ShowSearchAsync(IYandexMusicClient client, string query)
{
    var result = await client.Search.SearchAsync(query);
    if (result is null)
    {
        Console.WriteLine("Nothing found.");
        return;
    }

    PrintSection("Tracks", result.Tracks?.Results, t => $"{t.Title} — {FormatArtists(t.Artists)}");
    PrintSection("Albums", result.Albums?.Results, a => $"{a.Title} — {FormatArtists(a.Artists)}");
    PrintSection("Artists", result.Artists?.Results, a => a.Name);
}

static async Task ShowNewReleasesAsync(IYandexMusicClient client)
{
    var list = await client.Landing.GetNewReleasesAsync();
    var ids = list?.NewReleases ?? [];
    Console.WriteLine($"New releases: {ids.Count} album(s). Showing the first 10:");

    var albums = await client.Albums.GetManyAsync(ids.Take(10).Select(x => x.ToString(CultureInfo.InvariantCulture)));
    foreach (var a in albums)
    {
        Console.WriteLine($"  - {a.Title} — {FormatArtists(a.Artists)}");
    }
}

static async Task ShowChartAsync(IYandexMusicClient client, string region)
{
    var chart = await client.Landing.GetChartAsync(region);
    var playlist = chart?.Chart;
    if (playlist is null)
    {
        Console.WriteLine("Chart is unavailable.");
        return;
    }

    Console.WriteLine($"Chart: {chart!.Title} ({playlist.TrackCount} tracks)");
    var rank = 1;
    foreach (var entry in playlist.Tracks.Take(10))
    {
        var t = entry.Track;
        Console.WriteLine(t is null
            ? $"  {rank++,3}. (track {entry.Id})"
            : $"  {rank++,3}. {t.Title} — {FormatArtists(t.Artists)}");
    }
}

static async Task ShowLinkAsync(IYandexMusicClient client, string id)
{
    var link = await client.Tracks.GetDirectLinkAsync(id);
    Console.WriteLine(link is null
        ? $"No direct link for track {id} (a token with the right subscription is required)."
        : $"Direct link for track {id}:\n{link}");
}

static async Task ShowLikedAsync(IYandexMusicClient client, string? token)
{
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.Error.WriteLine("The `liked` command needs a token — set YANDEX_MUSIC_TOKEN.");
        return;
    }

    var status = await client.Account.GetStatusAsync();
    var uid = status?.Account.Uid.ToString(CultureInfo.InvariantCulture);
    if (string.IsNullOrEmpty(uid))
    {
        Console.WriteLine("Could not resolve the account.");
        return;
    }

    var liked = await client.Library.GetLikedTracksAsync(uid);
    var refs = liked?.Tracks ?? [];
    Console.WriteLine($"Liked tracks: {refs.Count}. Showing the first 10:");

    var full = await client.Tracks.GetManyAsync(refs.Take(10).Select(t => t.Id));
    foreach (var t in full)
    {
        Console.WriteLine($"  - {t.Title} — {FormatArtists(t.Artists)}");
    }
}

static void PrintSection<T>(string title, IReadOnlyList<T>? items, Func<T, string> format)
{
    if (items is not { Count: > 0 })
    {
        return;
    }

    Console.WriteLine($"\n{title}:");
    foreach (var item in items.Take(5))
    {
        Console.WriteLine($"  - {format(item)}");
    }
}

static string FormatArtists(IReadOnlyList<Artist> artists)
    => artists.Count == 0 ? "Unknown" : string.Join(", ", artists.Select(a => a.Name));

static string FormatDuration(long milliseconds)
{
    var span = TimeSpan.FromMilliseconds(milliseconds);
    return $"{(int)span.TotalMinutes}:{span.Seconds:D2}";
}

static void PrintHelp()
{
    Console.WriteLine(
        """
        YandexMusic sample CLI

        Usage: dotnet run --project samples/YandexMusic.Cli -- <command> [argument]

        Commands:
          track  <id>            a track and a few similar tracks         (default id: 4)
          tracks <id1,id2,...>   several tracks in one request            (default: 4,5,6)
          album  <id>            an album with its full tracklist         (default id: 3)
          artist <id>            an artist's popular tracks and albums    (default id: 79215)
          search <query>         search across tracks, albums and artists (default: Queen)
          new-releases           the latest released albums
          chart  [region]        the top chart                           (default: russia)
          link   <id>            a direct media link for a track          (needs a token)
          liked                  the signed-in user's liked tracks        (needs a token)

        Set YANDEX_MUSIC_TOKEN for the token-gated commands (link, liked).
        """);
}
