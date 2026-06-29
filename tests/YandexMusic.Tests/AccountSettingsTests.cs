using System.Text.Json;
using YandexMusic.Http;
using YandexMusic.Models.Account;
using YandexMusic.Serialization;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the account settings, purchase, experiment and promo-code models.</summary>
public sealed class AccountSettingsTests
{
    [Fact]
    public void Deserializes_AccountStatus_WithPlusAndSubscription()
    {
        const string json =
            """
            { "result": {
              "account": { "uid": 1, "login": "user", "registeredAt": "2015-01-01", "child": false,
                "now": "2024-01-01T00:00:00+03:00", "passportPhones": [ { "phone": "+70000000000" } ] },
              "plus": { "hasPlus": true, "isTutorialCompleted": true },
              "subscription": { "hadAnySubscription": true, "canStartTrial": false,
                "autoRenewable": [ { "expires": "2030-01-01", "vendor": "AppStore",
                  "vendorHelpUrl": "https://help", "finished": false } ],
                "operator": [ { "productId": "p1", "phone": "+7", "paymentRegularity": "monthly",
                  "title": "Carrier", "suspended": false,
                  "deactivation": [ { "method": "ussd", "instructions": "dial *100#" } ] } ] },
              "barBelow": { "alertId": "a1", "text": "Renew", "closeButton": true,
                "button": { "text": "Pay", "uri": "https://pay" } },
              "skipsPerHour": 6, "stationExists": true, "defaultEmail": "u@ya.ru" } }
            """;

        var status = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<AccountStatus>>())!.Result;

        Assert.NotNull(status);
        Assert.True(status!.Plus!.HasPlus);
        Assert.True(status.Subscription!.HadAnySubscription);
        Assert.Equal("AppStore", Assert.Single(status.Subscription.AutoRenewable!).Vendor);
        var op = Assert.Single(status.Subscription.Operator!);
        Assert.Equal("ussd", Assert.Single(op.Deactivation!).Method);
        Assert.Equal("Pay", status.BarBelow!.Button!.Text);
        Assert.Equal("2024-01-01T00:00:00+03:00", status.Account.Now);
        Assert.Equal("+70000000000", Assert.Single(status.Account.PassportPhones!).Phone);
        Assert.Equal(6, status.SkipsPerHour);
        Assert.Equal("u@ya.ru", status.DefaultEmail);
    }

    [Fact]
    public void Deserializes_PurchaseSettings()
    {
        const string json =
            """
            { "result": {
              "inAppProducts": [ {
                "productId": "yearly", "type": "subscription", "duration": 365, "trialDuration": 7,
                "feature": "music-player", "debug": false, "plus": true,
                "price": { "amount": 1990, "currency": "RUB" },
                "licenceTextParts": [ { "text": "Terms", "url": "https://ya.ru" } ]
              } ],
              "nativeProducts": [],
              "webPaymentUrl": "https://pay.ya.ru",
              "promoCodesEnabled": true,
              "webPaymentMonthProductPrice": { "amount": 299, "currency": "RUB" }
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<Settings>>());
        var settings = response!.Result!;

        var product = Assert.Single(settings.InAppProducts);
        Assert.Equal("yearly", product.ProductId);
        Assert.True(product.Plus);
        Assert.Equal(1990, product.Price!.Amount);
        Assert.Equal("Terms", product.LicenceTextParts![0].Text);
        Assert.True(settings.PromoCodesEnabled);
        Assert.Equal(299, settings.WebPaymentMonthProductPrice!.Amount);
    }

    [Fact]
    public void Deserializes_PromoCodeStatus_WithSnakeCaseAccountStatus()
    {
        const string json =
            """
            { "result": {
              "status": "code-accepted",
              "statusDesc": "Promo code accepted",
              "account_status": { "account": { "uid": 7, "login": "u" } }
            } }
            """;

        var response = JsonSerializer.Deserialize(json, YandexMusicJson.TypeInfo<ApiResponse<PromoCodeStatus>>());
        var status = response!.Result!;

        Assert.Equal("code-accepted", status.Status);
        Assert.Equal("Promo code accepted", status.StatusDesc);
        Assert.Equal(7, status.AccountStatus!.Account.Uid);
    }

    [Fact]
    public void Deserializes_ExperimentsDetails_WithExtensionData()
    {
        const string json =
            """
            { "result": {
              "new-ui": { "group": "B", "value": { "title": "B", "color": "red" } }
            } }
            """;

        var response = JsonSerializer.Deserialize(
            json,
            YandexMusicJson.TypeInfo<ApiResponse<Dictionary<string, ExperimentDetail>>>());
        var details = response!.Result!;

        var detail = details["new-ui"];
        Assert.Equal("B", detail.Group);
        Assert.Equal("B", detail.Value!.Title);
        Assert.Equal("red", detail.Value.AdditionalProperties!["color"].GetString());
    }
}
