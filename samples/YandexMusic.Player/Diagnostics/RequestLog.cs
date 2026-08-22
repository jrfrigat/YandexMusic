using System.Globalization;
using System.Text;

namespace YandexMusic.Player.Diagnostics;

/// <summary>
/// The player's request journal: an append-only text file recording the HTTP traffic and the raw
/// Ynison frames while it is switched on. It exists to answer "what did the server actually send",
/// so it is off by default, toggled from the main menu, and every line goes through
/// <see cref="Redaction"/> before it is written — a log meant to be shared must not carry the
/// account's credentials.
/// </summary>
public sealed class RequestLog : IDisposable
{
    private readonly Lock _gate = new();
    private StreamWriter? _writer;
    private bool _disposed;

    /// <summary>Creates the journal, pointed at the player's own data directory.</summary>
    public RequestLog()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "yandexmusic-player");
        FilePath = Path.Combine(directory, "requests.log");
    }

    /// <summary>Where the journal is written.</summary>
    public string FilePath { get; }

    /// <summary>Whether recording is currently on.</summary>
    public bool IsEnabled
    {
        get
        {
            lock (_gate)
            {
                return _writer is not null;
            }
        }
    }

    /// <summary>Turns recording on, appending to any earlier journal.</summary>
    /// <returns><see langword="true"/> when recording started; <see langword="false"/> when the file could not be opened.</returns>
    public bool Enable()
    {
        lock (_gate)
        {
            if (_disposed || _writer is not null)
            {
                return _writer is not null;
            }

            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                _writer = new StreamWriter(
                    new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read),
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                {
                    AutoFlush = true,
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _writer = null;
                return false;
            }

            WriteLocked("session", $"---- recording started, player {Environment.Version} ----");
            return true;
        }
    }

    /// <summary>Turns recording off and closes the file.</summary>
    public void Disable()
    {
        lock (_gate)
        {
            if (_writer is null)
            {
                return;
            }

            WriteLocked("session", "---- recording stopped ----");
            _writer.Dispose();
            _writer = null;
        }
    }

    /// <summary>Turns recording on when it is off and off when it is on.</summary>
    /// <returns>Whether recording is on afterwards.</returns>
    public bool Toggle()
    {
        if (IsEnabled)
        {
            Disable();
            return false;
        }

        return Enable();
    }

    /// <summary>Appends one entry; a no-op while recording is off.</summary>
    /// <param name="category">The traffic kind, for example <c>http</c> or <c>ynison</c>.</param>
    /// <param name="message">The text to record. It is redacted before it is written.</param>
    public void Write(string category, string message)
    {
        lock (_gate)
        {
            if (_writer is not null)
            {
                WriteLocked(category, message);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _writer?.Dispose();
            _writer = null;
        }
    }

    private void WriteLocked(string category, string message)
    {
        var stamp = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        _writer!.WriteLine($"[{stamp}] [{category}] {Redaction.Scrub(message)}");
    }
}
