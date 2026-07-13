using QRCoder;
using QRCoderSamples;

// Self-checking harness for OtpQrGenerator.
// Run with:  dotnet run --project QRCoderSamples -p:EnforceCodeStyleInBuild=false
// Verifies the payload is well-formed and that each image format produces valid bytes.

int failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"  [{(condition ? "PASS" : "FAIL")}] {name}");
    if (!condition)
        failures++;
}

Console.WriteLine("Testing OtpQrGenerator...");

// 1. The underlying otpauth payload should be correctly formed.
var otp = new PayloadGenerator.OneTimePassword
{
    Secret = "JBSWY3DPEHPK3PXP",
    Issuer = "MyApp",
    Label = "alice@example.com",
};
string payload = otp.ToString();
Console.WriteLine($"  payload = {payload}");
Check("payload has expected otpauth format",
    payload == "otpauth://totp/MyApp:alice%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=MyApp");

// 2. Each format should produce valid, recognizable bytes, and write a real file.
// Each entry: format, and a predicate that confirms the bytes look like that format.
var formats = new (QrImageFormat Format, Func<byte[], bool> LooksValid)[]
{
    // PNG: 8-byte file signature.
    (QrImageFormat.Png, b => b.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })),
    // BMP: starts with ASCII "BM".
    (QrImageFormat.Bmp, b => b.Length > 2 && b[0] == (byte)'B' && b[1] == (byte)'M'),
    // PDF: starts with "%PDF".
    (QrImageFormat.Pdf, b => b.Take(4).SequenceEqual(new byte[] { (byte)'%', (byte)'P', (byte)'D', (byte)'F' })),
    // SVG: XML text containing an <svg tag.
    (QrImageFormat.Svg, b => System.Text.Encoding.UTF8.GetString(b).Contains("<svg")),
    // JPEG: starts with FF D8 FF.
    (QrImageFormat.Jpeg, b => b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF),
};

string outDir = AppContext.BaseDirectory;

foreach (var (format, looksValid) in formats)
{
    byte[] bytes = OtpQrGenerator.CreateTotpQr(
        secret: "JBSWY3DPEHPK3PXP",
        issuer: "MyApp",
        label: "alice@example.com",
        format: format);

    Check($"{format} returns non-empty bytes", bytes.Length > 0);
    Check($"{format} output has a valid {format} header", looksValid(bytes));

    string outPath = Path.Combine(outDir, "otp-qr" + OtpQrGenerator.FileExtension(format));
    File.WriteAllBytes(outPath, bytes);
    Console.WriteLine($"  wrote {outPath}");
}

// 3. Grid: four accounts combined into ONE image, in each raster format.
var gridAccounts = new (string, string, string)[]
{
    ("JBSWY3DPEHPK3PXP", "MyApp", "alice@example.com"),
    ("KRSXG5CTMVRXEZLU", "MyApp", "bob@example.com"),
    ("MFRGGZDFMZTWQ2LK", "MyApp", "carol@example.com"),
    ("NB2W45DFOIZAEBZW", "MyApp", "dave@example.com"),
};
// Grids are raster-only (Bitmap-based), so skip the vector/document formats.
foreach (var (format, looksValid) in formats.Where(f => f.Format is QrImageFormat.Png or QrImageFormat.Jpeg or QrImageFormat.Bmp))
{
    byte[] grid = OtpQrGenerator.CreateTotpGrid(gridAccounts, format: format);
    Check($"Grid ({format}) has a valid {format} header", looksValid(grid));
    string gridPath = Path.Combine(outDir, "otp-grid" + OtpQrGenerator.FileExtension(format));
    File.WriteAllBytes(gridPath, grid);
    Console.WriteLine($"  wrote {gridPath} ({grid.Length} bytes)");
}

// 4. HOTP path in the default (PNG) format still works.
byte[] hotp = OtpQrGenerator.CreateHotpQr(
    secret: "JBSWY3DPEHPK3PXP",
    issuer: "MyApp",
    label: "bob@example.com",
    counter: 0);
Check("HOTP output is a valid PNG",
    hotp.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }));

Console.WriteLine(failures == 0
    ? "\nAll checks passed."
    : $"\n{failures} check(s) FAILED.");

// Non-zero exit code on failure so CI / the terminal reflects it.
return failures;
