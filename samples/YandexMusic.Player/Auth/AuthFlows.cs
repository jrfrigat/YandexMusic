using System.Diagnostics;
using Spectre.Console;
using YandexMusic;
using YandexMusic.Player.Ui;

namespace YandexMusic.Player.Auth;

/// <summary>Signs in by pasting an existing OAuth token — the most robust method.</summary>
public sealed class TokenAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => "OAuth token";

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        var token = AnsiConsole.Prompt(new TextPrompt<string>("Paste your [yellow]OAuth token[/]:").Secret());
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        client.Authentication.SignInWithToken(token.Trim());
        return await AuthSupport.ValidateAsync(client, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Signs in by rendering a QR code the user scans with the Yandex app.</summary>
public sealed class QrAuthFlow : IAuthFlow
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

    /// <inheritdoc />
    public string Name => "QR code";

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        var qr = await client.Authentication.StartQrSignInAsync(cancellationToken).ConfigureAwait(false);

        AnsiConsole.MarkupLine("[grey]Scan this with the Yandex app:[/]");
        AnsiConsole.WriteLine(QrRenderer.Render(qr.Url));
        AnsiConsole.MarkupLine($"[grey]…or open:[/] [blue underline]{Markup.Escape(qr.Url)}[/]");

        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Waiting for confirmation…", async _ =>
            {
                var stopwatch = Stopwatch.StartNew();
                while (!cancellationToken.IsCancellationRequested && stopwatch.Elapsed < Timeout)
                {
                    if (await client.Authentication.TryCompleteQrSignInAsync(qr, cancellationToken).ConfigureAwait(false))
                    {
                        return true;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken).ConfigureAwait(false);
                }

                return false;
            }).ConfigureAwait(false);
    }
}

/// <summary>Signs in via the official OAuth device-code flow (open a page, enter a short code).</summary>
public sealed class DeviceCodeAuthFlow : IAuthFlow
{
    /// <inheritdoc />
    public string Name => "Device code (open a page, enter a code)";

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await client.Authentication.SignInWithDeviceFlowAsync(
                code =>
                {
                    AnsiConsole.MarkupLine($"Open [blue underline]{Markup.Escape(code.VerificationUrl)}[/] and enter code [yellow]{Markup.Escape(code.UserCode)}[/]");
                    AnsiConsole.WriteLine(QrRenderer.Render(code.VerificationUrl));
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return token is not null;
        }
        catch (YandexMusicException ex)
        {
            AnsiConsole.MarkupLine($"[red]Device sign-in failed:[/] {Markup.Escape(ex.Message)}");
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
    public string Name => "Login + password (best-effort)";

    /// <inheritdoc />
    public async Task<bool> SignInAsync(IYandexMusicClient client, CancellationToken cancellationToken = default)
    {
        var login = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Login[/]:"));
        var password = AnsiConsole.Prompt(new TextPrompt<string>("[yellow]Password[/]:").Secret());

        try
        {
            await client.Authentication.SignInWithPasswordAsync(login, password, cancellationToken).ConfigureAwait(false);
        }
        catch (YandexMusicException ex)
        {
            AnsiConsole.MarkupLine($"[red]Password sign-in failed:[/] {Markup.Escape(ex.Message)}");
            AnsiConsole.MarkupLine("[grey]If 2FA or a captcha is enabled, use the QR or device-code method.[/]");
            return false;
        }

        return await AuthSupport.ValidateAsync(client, cancellationToken).ConfigureAwait(false);
    }
}
