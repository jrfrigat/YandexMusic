using Spectre.Console;
using YandexMusic;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Auth;

/// <summary>
/// Coordinates sign-in: first tries to restore a persisted session, and otherwise lets the user pick
/// one of the registered <see cref="IAuthFlow"/> methods. On success the session is persisted so the
/// next run starts already signed in.
/// </summary>
public sealed class AuthService
{
    private readonly IReadOnlyList<IAuthFlow> _flows;
    private readonly ISessionStore _store;

    /// <summary>Creates the auth service.</summary>
    /// <param name="flows">The available sign-in methods.</param>
    /// <param name="store">The session store.</param>
    public AuthService(IEnumerable<IAuthFlow> flows, ISessionStore store)
    {
        ArgumentNullException.ThrowIfNull(flows);
        ArgumentNullException.ThrowIfNull(store);
        _flows = flows.ToList();
        _store = store;
    }

    /// <summary>Ensures the client is signed in, restoring a session or running a chosen flow.</summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns><see langword="true"/> when signed in; <see langword="false"/> when the user gave up.</returns>
    public async Task<bool> EnsureSignedInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        if (await TryRestoreAsync(client, cancellationToken).ConfigureAwait(false))
        {
            return true;
        }

        var quit = Strings.AuthQuit;
        while (!cancellationToken.IsCancellationRequested)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(Strings.HowToSignIn)
                    .AddChoices(_flows.Select(f => f.Name).Append(quit)));

            if (choice == quit)
            {
                return false;
            }

            var flow = _flows.First(f => f.Name == choice);

            try
            {
                if (await flow.SignInAsync(client, cancellationToken).ConfigureAwait(false))
                {
                    _store.Save(client.Authentication.Session.Export());
                    AnsiConsole.MarkupLine(Strings.SignedIn);
                    return true;
                }

                AnsiConsole.MarkupLine(Strings.SignInIncomplete);
            }
            catch (YandexMusicException ex)
            {
                AnsiConsole.MarkupLine(Strings.SignInFailed(Markup.Escape(ex.Message)));
            }
        }

        return false;
    }

    /// <summary>Signs out and forgets the persisted session.</summary>
    /// <param name="client">The client to sign out.</param>
    public void SignOut(IYandexMusicClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.Authentication.SignOut();
        _store.Clear();
    }

    private async Task<bool> TryRestoreAsync(IYandexMusicClient client, CancellationToken cancellationToken)
    {
        var snapshot = _store.Load();
        if (snapshot is null)
        {
            return false;
        }

        client.Authentication.Session.Import(snapshot);
        if (await AuthSupport.ValidateAsync(client, cancellationToken).ConfigureAwait(false))
        {
            return true;
        }

        // Stale session — drop it and fall back to interactive sign-in.
        _store.Clear();
        client.Authentication.SignOut();
        return false;
    }
}
