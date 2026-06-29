using System.Globalization;
using Xunit;

namespace YandexMusic.Tests.Integration;

/// <summary>
/// End-to-end tests against the live Yandex Music API. They run only when <c>YANDEX_MUSIC_TOKEN</c>
/// is set (see <see cref="IntegrationFactAttribute"/>) and validate the full stack — including the
/// personal account/library endpoints and the signed download-link resolution.
/// </summary>
public sealed class LiveApiTests : IClassFixture<YandexMusicFixture>
{
    private readonly YandexMusicFixture _fixture;

    /// <summary>Initializes the test class with the shared fixture.</summary>
    /// <param name="fixture">The signed-in client fixture.</param>
    public LiveApiTests(YandexMusicFixture fixture) => _fixture = fixture;

    [IntegrationFact]
    public async Task GetTrack_ReturnsMetadata()
    {
        var track = await _fixture.Client!.Tracks.GetAsync("4");

        Assert.NotNull(track);
        Assert.Equal("Just In Time", track!.Title);
        Assert.NotEmpty(track.Artists);
    }

    [IntegrationFact]
    public async Task Search_ReturnsArtists()
    {
        var result = await _fixture.Client!.Search.SearchAsync("queen");

        Assert.NotNull(result);
        Assert.NotNull(result!.Artists);
        Assert.NotEmpty(result.Artists!.Results);
    }

    [IntegrationFact]
    public async Task Suggest_ReturnsSuggestions()
    {
        var suggestions = await _fixture.Client!.Search.SuggestAsync("queen");

        Assert.NotNull(suggestions);
        Assert.NotEmpty(suggestions!.Suggestions);
    }

    [IntegrationFact]
    public async Task AccountStatus_ReturnsSignedInUser()
    {
        var status = await _fixture.Client!.Account.GetStatusAsync();

        Assert.NotNull(status);
        Assert.True(status!.Account.Uid > 0, "Expected an authenticated account with a real uid.");
    }

    [IntegrationFact]
    public async Task LikedTracks_AreReturnedForCurrentUser()
    {
        var status = await _fixture.Client!.Account.GetStatusAsync();
        var uid = status!.Account.Uid.ToString(CultureInfo.InvariantCulture);

        var library = await _fixture.Client!.Library.GetLikedTracksAsync(uid);

        Assert.NotNull(library);
        Assert.Equal(status.Account.Uid, library!.Uid);
    }

    [IntegrationFact]
    public async Task GetDirectLink_ResolvesSignedUrl()
    {
        var url = await _fixture.Client!.Tracks.GetDirectLinkAsync("4");

        Assert.NotNull(url);
        Assert.StartsWith("https://", url, StringComparison.Ordinal);
        Assert.Contains("/get-mp3/", url, StringComparison.Ordinal);
    }
}
