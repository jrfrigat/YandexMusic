using Spectre.Console;
using YandexMusic;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Auth;

/// <summary>Signs in by pasting an existing OAuth token — the most robust method.</summary>
public sealed class TokenAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => Strings.FlowToken;

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        var token = AnsiConsole.Prompt(new TextPrompt<string>(Strings.PromptToken).Secret());
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        client.Authentication.SignInWithToken(token.Trim());
        return await AuthSupport.ValidateAsync(client, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Signs in by rendering a QR code for the OAuth device-code verification page. This uses the official,
/// captcha-free device-code flow (not the brittle Passport magic-link, which Yandex gates behind a
/// captcha): the user scans the code, opens the page, enters the short code, and the app polls until
/// the sign-in is confirmed.
/// </summary>
public sealed class QrAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => Strings.FlowQr;

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await client.Authentication.SignInWithDeviceFlowAsync(
                code =>
                {
                    AnsiConsole.MarkupLine(Strings.ScanWithApp);
                    AnsiConsole.WriteLine(QrRenderer.Render(code.VerificationUrl));
                    AnsiConsole.MarkupLine(Strings.OrOpen(Markup.Escape(code.VerificationUrl)));
                    AnsiConsole.MarkupLine(Strings.EnterCode(Markup.Escape(code.UserCode)));
                    AnsiConsole.MarkupLine($"[grey]{Strings.WaitingConfirmation}[/]");
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return token is not null;
        }
        catch (YandexMusicException ex)
        {
            AnsiConsole.MarkupLine(Strings.DeviceSignInFailed(Markup.Escape(ex.Message)));
            return false;
        }
    }
}

/// <summary>Signs in via the official OAuth device-code flow (open a page, enter a short code).</summary>
public sealed class DeviceCodeAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => Strings.FlowDevice;

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await client.Authentication.SignInWithDeviceFlowAsync(
                code =>
                {
                    AnsiConsole.MarkupLine(Strings.OpenAndEnterCode(Markup.Escape(code.VerificationUrl), Markup.Escape(code.UserCode)));
                    AnsiConsole.MarkupLine($"[grey]{Strings.WaitingConfirmation}[/]");
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return token is not null;
        }
        catch (YandexMusicException ex)
        {
            AnsiConsole.MarkupLine(Strings.DeviceSignInFailed(Markup.Escape(ex.Message)));
            return false;
        }
    }
}

/// <summary>
/// Signs in with a login and password. Best-effort: Yandex frequently requires a captcha or 2FA, in
/// which case this fails and the user should use the QR or device-code method instead.
/// </summary>
public sealed class PasswordAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => Strings.FlowPassword;

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        var login = AnsiConsole.Prompt(new TextPrompt<string>(Strings.PromptLogin));
        var password = AnsiConsole.Prompt(new TextPrompt<string>(Strings.PromptPassword).Secret());

        try
        {
            await client.Authentication.SignInWithPasswordAsync(login, password, cancellationToken).ConfigureAwait(false);
        }
        catch (YandexMusicException ex)
        {
            AnsiConsole.MarkupLine(Strings.PasswordSignInFailed(Markup.Escape(ex.Message)));
            AnsiConsole.MarkupLine(Strings.PasswordHint);
            return false;
        }

        return await AuthSupport.ValidateAsync(client, cancellationToken).ConfigureAwait(false);
    }
}
