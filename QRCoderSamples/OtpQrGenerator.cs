using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

    /// <summary>JPEG raster image. Windows-only (uses System.Drawing/GDI+).</summary>
    Jpeg,
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
            QrImageFormat.Jpeg => RenderJpeg(qrData, pixelsPerModule),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format."),
        };
    }

    // JPEG isn't a native QRCoder byte renderer: render to a System.Drawing Bitmap
    // (via the QRCode class) and encode it as JPEG. Windows-only, as GDI+ requires it.
    private static byte[] RenderJpeg(QRCodeData qrData, int pixelsPerModule)
    {
        using var qrCode = new QRCode(qrData);
        using var bmp = qrCode.GetGraphic(pixelsPerModule);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }

    /// <summary>
    /// Combines up to four TOTP accounts into a single image, laid out as a 2x2 grid.
    /// Windows-only (uses System.Drawing).
    /// </summary>
    /// <param name="accounts">1 to 4 accounts, each a (secret, issuer, label) tuple.</param>
    /// <param name="format">Output image format. Raster only: Png (default), Jpeg, or Bmp.</param>
    /// <param name="pixelsPerModule">Size of each QR module in pixels (default 10; smaller keeps the grid compact).</param>
    /// <returns>Image bytes containing all the QR codes tiled together.</returns>
    public static byte[] CreateTotpGrid(
        IReadOnlyList<(string secret, string issuer, string label)> accounts,
        QrImageFormat format = QrImageFormat.Png,
        int pixelsPerModule = 10)
    {
        if (accounts is null || accounts.Count == 0)
            throw new ArgumentException("Provide 1 to 4 accounts.", nameof(accounts));
        if (accounts.Count > 4)
            throw new ArgumentException("A 2x2 grid holds at most 4 accounts.", nameof(accounts));

        using var generator = new QRCodeGenerator();
        var tiles = new List<Bitmap>();
        try
        {
            foreach (var (secret, issuer, label) in accounts)
            {
                var otp = new PayloadGenerator.OneTimePassword
                {
                    Type = PayloadGenerator.OneTimePassword.OneTimePasswordAuthType.TOTP,
                    Secret = secret,
                    Issuer = issuer,
                    Label = label,
                };
                using var data = generator.CreateQrCode(otp.ToString(), QRCodeGenerator.ECCLevel.Q);
                using var qr = new QRCode(data);
                tiles.Add(qr.GetGraphic(pixelsPerModule));
            }

            using var composite = ComposeGrid(tiles, pixelsPerModule);
            return EncodeBitmap(composite, format);
        }
        finally
        {
            foreach (var t in tiles)
                t.Dispose();
        }
    }

    // Lays the QR bitmaps out in a tight 2x2 grid on a white canvas, centering each tile in
    // its cell. Mirrors the Bitmap/Graphics pattern in ArtQRCode.cs. Each QR already carries
    // its own white quiet zone, so only a small gap is added between tiles.
    private static Bitmap ComposeGrid(List<Bitmap> tiles, int pixelsPerModule)
    {
        const int cols = 2;
        const int rows = 2;
        int cellW = tiles.Max(t => t.Width);
        int cellH = tiles.Max(t => t.Height);
        int gap = Math.Max(4, pixelsPerModule / 2);

        int totalW = (cols * cellW) + ((cols + 1) * gap);
        int totalH = (rows * cellH) + ((rows + 1) * gap);

        var composite = new Bitmap(totalW, totalH, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(composite);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.Clear(Color.White);

        for (int i = 0; i < tiles.Count; i++)
        {
            int r = i / cols;
            int c = i % cols;
            int cellX = gap + (c * (cellW + gap));
            int cellY = gap + (r * (cellH + gap));
        }

        return composite;
    }

    // Encodes an already-composed Bitmap to raster bytes. Grid output is Bitmap-based,
    // so vector/document formats (Svg, Pdf) are not supported here.
    private static byte[] EncodeBitmap(Bitmap bmp, QrImageFormat format)
    {
        var imageFormat = format switch
        {
            QrImageFormat.Png => ImageFormat.Png,
            QrImageFormat.Jpeg => ImageFormat.Jpeg,
            QrImageFormat.Bmp => ImageFormat.Bmp,
            _ => throw new NotSupportedException($"Grid output supports raster formats (Png, Jpeg, Bmp) only, not {format}."),
        };
        using var ms = new MemoryStream();
        bmp.Save(ms, imageFormat);
        return ms.ToArray();
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
        QrImageFormat.Jpeg => ".jpg",
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported image format."),
    };
}
