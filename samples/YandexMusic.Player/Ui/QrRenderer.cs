using QRCoder;

namespace YandexMusic.Player.Ui;

/// <summary>Renders a QR code as text blocks suitable for printing in a terminal.</summary>
public static class QrRenderer
{
    /// <summary>Renders <paramref name="text"/> as a scannable block-character QR code.</summary>
    /// <param name="text">The payload to encode (usually a URL).</param>
    /// <returns>A multi-line string drawing the QR code.</returns>
    public static string Render(string text)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.L);
        return new AsciiQRCode(data).GetGraphic(1, "██", "  ");
    }
}
