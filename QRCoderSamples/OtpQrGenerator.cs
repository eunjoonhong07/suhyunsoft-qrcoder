using System.Text;
using QRCoder;

namespace QRCoderSamples;

/// <summary>
/// Image formats supported by <see cref="OtpQrGenerator"/>.
/// </summary>
public enum QrImageFormat
{
    /// <summary>PNG raster image (default). Universally supported.</summary>
    Png,

    /// <summary>SVG vector image. Scales to any size without quality loss; great for web.</summary>
    Svg,

    /// <summary>BMP raster image. Uncompressed; large files, but no dependencies.</summary>
    Bmp,

    /// <summary>PDF document containing the QR code.</summary>
    Pdf,
}

/// <summary>
/// Reusable helper for producing QR codes that provision one-time-password
/// (TOTP/HOTP) accounts in authenticator apps such as Google Authenticator or Authy.
/// </summary>
public static class OtpQrGenerator
{
    /// <summary>
    /// Builds a QR code for a TOTP account in the requested image format.
    /// </summary>
    /// <param name="secret">Base32-encoded shared secret (A-Z, 2-7). Spaces are ignored.</param>
    /// <param name="issuer">Your service/company name (must not contain ':').</param>
    /// <param name="label">The user's account, e.g. their email (must not contain ':').</param>
    /// <param name="format">Output image format (default PNG).</param>
    /// <param name="pixelsPerModule">Size of each QR module in pixels. Larger = higher resolution.</param>
    /// <param name="digits">Number of code digits (default 6).</param>
    /// <param name="periodSeconds">TOTP refresh period in seconds (default 30).</param>
    /// <param name="algorithm">Hashing algorithm (default SHA1, the authenticator standard).</param>
    /// <returns>Image bytes in the requested format. Write to a file or stream, or embed as base64.</returns>
    public static byte[] CreateTotpQr(
        string secret,
        string issuer,
        string label,
        QrImageFormat format = QrImageFormat.Png,
        int pixelsPerModule = 20,
        int digits = 6,
        int periodSeconds = 30,
        PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm algorithm
            = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA1)
    {
        var otp = new PayloadGenerator.OneTimePassword
        {
            Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.TOTP,
            Secret = secret,
            Issuer = issuer,
            Label = label,
            Digits = digits,
            Period = periodSeconds,
            AuthAlgorithm = algorithm,
        };

        return Render(otp.ToString(), format, pixelsPerModule);
    }

    /// <summary>
    /// Builds a QR code for a counter-based HOTP account in the requested image format.
    /// </summary>
    /// <param name="secret">Base32-encoded shared secret (A-Z, 2-7). Spaces are ignored.</param>
    /// <param name="issuer">Your service/company name (must not contain ':').</param>
    /// <param name="label">The user's account, e.g. their email (must not contain ':').</param>
    /// <param name="counter">Initial HOTP counter value.</param>
    /// <param name="format">Output image format (default PNG).</param>
    /// <param name="pixelsPerModule">Size of each QR module in pixels.</param>
    /// <param name="digits">Number of code digits (default 6).</param>
    /// <param name="algorithm">Hashing algorithm (default SHA1).</param>
    /// <returns>Image bytes in the requested format.</returns>
    public static byte[] CreateHotpQr(
        string secret,
        string issuer,
        string label,
        int counter,
        QrImageFormat format = QrImageFormat.Png,
        int pixelsPerModule = 20,
        int digits = 6,
        PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm algorithm
            = PayloadGenerator.OneTimePassword.OneTimePasswordAuthAlgorithm.SHA1)
    {
        var otp = new PayloadGenerator.OneTimePassword
        {
            Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.HOTP,
            Secret = secret,
            Issuer = issuer,
            Label = label,
            Counter = counter,
            Digits = digits,
            AuthAlgorithm = algorithm,
        };

        return Render(otp.ToString(), format, pixelsPerModule);
    }

    /// <summary>
    /// Renders any otpauth:// payload string to image bytes in the requested format.
    /// Shared by the TOTP/HOTP helpers; also usable directly if you build the payload yourself.
    /// </summary>
    public static byte[] Render(string otpauthPayload, QrImageFormat format = QrImageFormat.Png, int pixelsPerModule = 20)
    {
        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(otpauthPayload, QRCodeGenerator.ECCLevel.Q);

        return format switch
        {
            QrImageFormat.Png => new PngByteQRCode(qrData).GetGraphic(pixelsPerModule),
            QrImageFormat.Bmp => new BitmapByteQRCode(qrData).GetGraphic(pixelsPerModule),
            QrImageFormat.Pdf => new PdfByteQRCode(qrData).GetGraphic(pixelsPerModule),
            // SvgQRCode returns a string; encode it as UTF-8 bytes so the API stays uniform.
            QrImageFormat.Svg => Encoding.UTF8.GetBytes(new SvgQRCode(qrData).GetGraphic(pixelsPerModule)),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format."),
        };
    }

    /// <summary>
    /// Returns the conventional file extension (including the dot) for a format, e.g. ".png".
    /// Handy when building an output file path.
    /// </summary>
    public static string FileExtension(QrImageFormat format) => format switch
    {
        QrImageFormat.Png => ".png",
        QrImageFormat.Svg => ".svg",
        QrImageFormat.Bmp => ".bmp",
        QrImageFormat.Pdf => ".pdf",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format."),
    };
}
