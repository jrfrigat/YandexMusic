using Spectre.Console;
using YandexMusic;
using YandexMusicTerminal.Diagnostics;
using YandexMusicTerminal.Playback;
using YandexMusicTerminal.Remote;
using YandexMusicTerminal.Ui;

namespace YandexMusicTerminal.Screens;

/// <summary>
/// Hands the track the player is on to a speaker on this network: pick one, and it carries on there
/// while this player is free to stop.
///
/// Only speakers are offered. A device in the account's Ynison session can be told to start playing,
/// but not what to play — so listing one here would promise something that cannot be delivered.
/// </summary>
public sealed class SendToDeviceScreen
{
    private readonly IYandexMusicClient _client;
    private readonly PlaybackController _controller;
    private readonly RequestLog _log;

    /// <summary>Creates the screen.</summary>
    /// <param name="client">The signed-in client, used to reach the account's device list.</param>
    /// <param name="controller">The player's playback, whose current track is the one handed over.</param>
    /// <param name="log">The request journal the raw device frames go to.</param>
    public SendToDeviceScreen(IYandexMusicClient client, PlaybackController controller, RequestLog log)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(log);
        _client = client;
        _controller = controller;
        _log = log;
    }

    /// <summary>Scans, asks which speaker, and sends the current track to it.</summary>
    /// <param name="cancellationToken">A token to cancel.</param>
    /// <returns>A message for the caller's toast, or <see langword="null"/> when the user backed out.</returns>
    public async Task<string?> RunAsync(CancellationToken cancellationToken = default)
    {
        if (_controller.Current is not { } track)
        {
            return Strings.HandOverNothingPlaying;
        }

        await using var speakers = new LocalSpeakers(_client, _log);

        // The scan is short and the user is waiting on it, so this one is awaited rather than left to
        // fill a list in the background the way the remote does it.
        await AnsiConsole.Status()
            .StartAsync(Strings.RemoteLocalScanning, async _ =>
            {
                speakers.StartScan(cancellationToken);
                while (speakers.IsScanning && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(200, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

        var found = speakers.Found;
        if (found.Count == 0)
        {
            return Strings.RemoteLocalNone;
        }

        var view = new SelectionView<LocalSpeaker>(
            Strings.SendToDeviceTitle(Format.Truncate(track.Title, 40)),
            found,
            speaker => $"[white]{Markup.Escape(speaker.Name)}[/]  [grey]{Markup.Escape(speaker.Platform)}[/]");

        var chosen = await view.ShowAsync(cancellationToken).ConfigureAwait(false);
        if (chosen is null)
        {
            return null;
        }

        var failure = await speakers.ConnectAsync(chosen, cancellationToken).ConfigureAwait(false);
        if (speakers.Connected is null)
        {
            return failure;
        }

        if (await speakers.PlayTrackAsync(track.Id, cancellationToken).ConfigureAwait(false) is { } rejected)
        {
            return rejected;
        }

        var name = Format.Truncate(chosen.Name, 24);
        return await StartedAsync(speakers, track.Id, cancellationToken).ConfigureAwait(false)
            ? Strings.HandOverSent(Format.Truncate(track.Title, 34), name)
            : Strings.HandOverUnconfirmed(name);
    }

    /// <summary>
    /// Waits for the speaker to report that it really is on this track.
    ///
    /// The command is answered <c>SUCCESS</c> whether or not anything started — an unknown id gets
    /// the same answer as a good one — so the reply proves nothing. What proves it is the state the
    /// speaker pushes afterwards, roughly once a second while it plays.
    /// </summary>
    private static async Task<bool> StartedAsync(LocalSpeakers speakers, string trackId, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(6);
        while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            if (speakers.State?.State?.PlayerState?.Id == trackId)
            {
                return true;
            }

            await Task.Delay(300, cancellationToken).ConfigureAwait(false);
        }

        return false;
    }
}
