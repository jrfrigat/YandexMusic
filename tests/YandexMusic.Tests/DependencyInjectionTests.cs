using Microsoft.Extensions.DependencyInjection;
using YandexMusic;
using Xunit;

namespace YandexMusic.Tests;

/// <summary>Verifies the <c>AddYandexMusic</c> dependency-injection registration.</summary>
public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddYandexMusic_RegistersClientAsScoped()
    {
        var services = new ServiceCollection();
        services.AddYandexMusic();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IYandexMusicClient));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddYandexMusic_AppliesOptions()
    {
        using var provider = new ServiceCollection()
            .AddYandexMusic(o => o.DeviceId = "my-device")
            .BuildServiceProvider();

        Assert.Equal("my-device", provider.GetRequiredService<YandexMusicClientOptions>().DeviceId);
    }

    [Fact]
    public void Clients_FromDifferentScopes_AreDistinctAndIsolated()
    {
        using var provider = new ServiceCollection().AddYandexMusic().BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var first = scope1.ServiceProvider.GetRequiredService<IYandexMusicClient>();
        var second = scope2.ServiceProvider.GetRequiredService<IYandexMusicClient>();

        Assert.NotSame(first, second);
        Assert.NotSame(first.Authentication.Session, second.Authentication.Session);

        // Signing one scope's client in must not authenticate the other.
        first.Authentication.SignInWithToken("token-1");
        Assert.True(first.Authentication.IsAuthenticated);
        Assert.False(second.Authentication.IsAuthenticated);
    }

    [Fact]
    public void Client_WithinSameScope_IsSingleInstance()
    {
        using var provider = new ServiceCollection().AddYandexMusic().BuildServiceProvider();
        using var scope = provider.CreateScope();

        var a = scope.ServiceProvider.GetRequiredService<IYandexMusicClient>();
        var b = scope.ServiceProvider.GetRequiredService<IYandexMusicClient>();

        Assert.Same(a, b);
    }
}
