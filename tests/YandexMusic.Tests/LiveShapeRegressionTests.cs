using System.Text.Json;
using YandexMusic.Authentication;
using YandexMusic.Http;
using YandexMusic.Models.Radio;
using YandexMusic.Models.Search;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Regression tests for live-API response shapes that differ from the obvious model (found by running
/// the sample CLI against the real service): the same fields can arrive as numbers in one endpoint and
/// strings in another, and album labels can be bare strings in search results.
/// </summary>
public sealed class LiveShapeRegressionTests
{
    // Shared per-converter options so each test run reuses one instance (CA1869).
    private static readonly JsonSerializerOptions Int64Options = CreateOptions(new FlexibleInt64Converter());
    private static readonly JsonSerializerOptions DecimalOptions = CreateOptions(new FlexibleDecimalConverter());

    private static JsonSerializerOptions CreateOptions(System.Text.Json.Serialization.JsonConverter converter)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        return options;
    }

    [Theory]
    [InlineData("166", 166L)]      // JSON number
    [InlineData("\"166\"", 166L)]  // JSON string (search sends exec-duration-millis like this)
    [InlineData("\"abc\"", null)]  // unparseable string -> null, never throws
    [InlineData("null", null)]
    public void FlexibleInt64_ReadsStringOrNumber(string json, long? expected)
        => Assert.Equal(expected, JsonSerializer.Deserialize<long?>(json, Int64Options));

    [Fact]
    public void Search_DeserializesLiveShapes_NumericIdsStringLabelsStringDuration()
    {
        // Mirrors the awkward bits of a real /search response: a string exec-duration-millis, numeric
        // track/album/artist ids, and album labels as bare strings.
        const string json =
            """
            {
              "invocationInfo": { "exec-duration-millis": "166" },
              "result": {
                "tracks": { "results": [ { "id": 42, "title": "Song", "artists": [ { "id": 7, "name": "Artist" } ] } ] },
                "albums": { "results": [ { "id": 3, "title": "Album", "labels": [ "Some Label" ] } ] },
                "artists": { "results": [ { "id": 9, "name": "Solo" } ] }
              }
            }
            """;

        var envelope = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<SearchResult>>());

        Assert.Equal(166L, envelope!.InvocationInfo!.ExecDurationMillis);
        var result = envelope.Result!;

        var track = Assert.Single(result.Tracks!.Results);
        Assert.Equal("42", track.Id);
        Assert.Equal("7", Assert.Single(track.Artists).Id);

        var album = Assert.Single(result.Albums!.Results);
        Assert.Equal("3", album.Id);
        Assert.Equal("Some Label", Assert.Single(album.Labels!).Name);

        Assert.Equal("9", Assert.Single(result.Artists!.Results).Id);
    }

    [Theory]
    [InlineData("0", 0)]          // integer
    [InlineData("0.0", 0)]        // live account/status sends price.amount like this
    [InlineData("169.50", 169.50)]
    [InlineData("\"169\"", 169)]  // string form
    [InlineData("\"abc\"", 0)]    // unparseable -> 0, never throws
    [InlineData("null", 0)]
    public void FlexibleDecimal_ReadsNumberOrString(string json, double expected)
        => Assert.Equal((decimal)expected, JsonSerializer.Deserialize<decimal>(json, DecimalOptions));

    [Fact]
    public void AccountStatus_DeserializesFloatPriceAmount()
    {
        // Regression: with a browser User-Agent, /account/status returns subscription price amounts as
        // floating-point numbers ("amount": 0.0). A plain int property threw and rejected valid tokens.
        const string json =
            """
            {
              "result": {
                "account": { "uid": 1, "login": "user" },
                "subscription": {
                  "autoRenewable": [
                    { "product": { "productId": "p", "price": { "amount": 0.0, "currency": "no-information" } } }
                  ]
                }
              }
            }
            """;

        var status = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<YandexMusic.Models.Account.AccountStatus>>())!.Result!;

        var price = status.Subscription!.AutoRenewable![0].Product!.Price!;
        Assert.Equal(0m, price.Amount);
        Assert.Equal("no-information", price.Currency);
    }

    [Fact]
    public void Album_Labels_StillReadObjectForm()
    {
        // The non-search endpoints return labels as objects; that path must keep working.
        const string json =
            """
            { "result": { "id": 3, "title": "Album", "labels": [ { "id": 5, "name": "Object Label" } ] } }
            """;

        var album = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<YandexMusic.Models.Albums.Album>>())!.Result!;

        var label = Assert.Single(album.Labels!);
        Assert.Equal("Object Label", label.Name);
        Assert.Equal(5, label.Id);
    }

    [Fact]
    public void WaveSettings_DiscreteScaleBoundsArriveAsNumbers()
    {
        // /rotor/wave/settings sends the mood and energy scale bounds as JSON numbers.
        const string json =
            """
            { "result": { "blocks": [ { "type": "contexts", "items": [ { "name": "Work",
              "restrictions": { "mood": { "type": "discrete-scale", "name": "Mood",
                "min": { "value": 1, "name": "Sad" }, "max": { "value": 4, "name": "Fun" } } } } ] } ] } }
            """;

        var settings = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<WaveSettings>>())!.Result!;

        var mood = settings.Blocks![0].Items![0].Restrictions.Mood!;
        Assert.Equal("1", mood.Min!.Value);
        Assert.Equal("4", mood.Max!.Value);
    }

    [Fact]
    public async Task PerRequestHeader_ReplacesTheDefaultInsteadOfAddingASecondValue()
    {
        // The file-info signature is bound to one X-Yandex-Music-Client value; sent next to the
        // default device id, the server answers 403.
        using var handler = new CapturingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.music.yandex.net") };
        var connection = new YandexMusicConnection(http, new AuthSession("device-1"));

        await connection.GetAsync<JsonElement>("/get-file-info", new Dictionary<string, string> { ["X-Yandex-Music-Client"] = "Desktop/1" }, CancellationToken.None);

        Assert.Equal(["Desktop/1"], handler.Request!.Headers.GetValues("X-Yandex-Music-Client"));
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"result\":{}}", System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }
}
