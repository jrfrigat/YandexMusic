using YandexMusic.Player.Diagnostics;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>
/// Guards the request journal's redaction. A journal exists to be handed to someone else, so every
/// shape a credential arrives in has to be masked; these are the shapes the player actually sends
/// and receives.
/// </summary>
public sealed class RedactionTests
{
    [Theory]
    [InlineData("Authorization: OAuth y0__xDy2budARje-AYg7rmliBc11LbYoMeUiwiO6f6mSCAMDYVIKg")]
    [InlineData("    authorization: oauth AQAAAAABBBBCCCC")]
    [InlineData("Authorization: Bearer AQAAAAABBBBCCCC")]
    [InlineData("""{"access_token":"AQAAAAABBBBCCCC","token_type":"bearer"}""")]
    [InlineData("""{"token": "AQAAAAABBBBCCCC"}""")]
    [InlineData("grant_type=password&password=hunter2&access_token=AQAAAAABBBBCCCC")]
    [InlineData("Cookie: Session_id=3:abcdef; yandexuid=12345")]
    [InlineData("Set-Cookie: Session_id=3:abcdef; Path=/")]
    [InlineData("""{"password":"hunter2"}""")]
    [InlineData("""{"redirect_ticket":"ticket-1"}""")]
    public void Scrub_MasksEveryCredentialShape(string line)
    {
        var scrubbed = Redaction.Scrub(line);

        Assert.Contains("<redacted>", scrubbed, StringComparison.Ordinal);
        foreach (var secret in new[] { "y0__xDy2budARje", "AQAAAAABBBBCCCC", "hunter2", "3:abcdef", "ticket-1" })
        {
            Assert.DoesNotContain(secret, scrubbed, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Scrub_LeavesOrdinaryPayloadsReadable()
    {
        // The point of the journal is the payload; redaction must not eat it.
        const string frame =
            """{"devices":[{"info":{"device_id":"abc","title":"Station","type":"SMART_SPEAKER"}}],"rid":"r-1"}""";

        Assert.Equal(frame, Redaction.Scrub(frame));
    }

    [Fact]
    public void Scrub_HandlesEmptyInput()
    {
        Assert.Equal(string.Empty, Redaction.Scrub(null));
        Assert.Equal(string.Empty, Redaction.Scrub(string.Empty));
    }
}
