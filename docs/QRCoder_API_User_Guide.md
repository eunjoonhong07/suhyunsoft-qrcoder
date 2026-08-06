# QRCoder API User Guide v1.0

| Item | Description |
|------|-------------|
| Document Type | API User Manual |
| Target Reader | .NET developer integrating QRCoder.dll |
| DLL Name | QRCoder.dll |
| Version | 1.0.0 |

# 1. Introduction

## Overview

`QRCoder.dll` is a C# library for generating QR codes. This trimmed build focuses on
two-factor-authentication (TOTP/HOTP) QR codes via the One-Time-Password payload, on top of the
full QR-generation engine (which can also encode any arbitrary text or URL).

## Features

- QR code generation from any text or payload
- One-Time-Password (TOTP/HOTP) payload generation for authenticator apps
- Multiple output formats: PNG, SVG, BMP, PDF, and System.Drawing Bitmap (JPEG via the sample)
- Selectable error-correction levels (L / M / Q / H)
- Micro QR code support

## Supported Platforms

- .NET 8.0 or later (the DLL targets `net8.0`)
- Cross-platform for PNG / SVG / BMP / PDF renderers
- Windows only for the `QRCode` (System.Drawing / GDI+) renderer

# 2. Installation

This library is built from source (not published to NuGet). Reference it one of two ways:

- **Project reference:** add `QRCoder/QRCoder.csproj` to your solution and reference it.
- **Binary reference:** build the DLL and reference the produced file:
  `QRCoder/bin/Release/net8.0/QRCoder.dll`.

Then include the namespace in your source:

```csharp
using QRCoder;
```

# 3. Getting Started

## Typical Workflow

1. Build a payload (e.g. `PayloadGenerator.OneTimePassword`) or use plain text.
2. Create the QR data with `QRCodeGenerator.CreateQrCode(...)` → `QRCodeData`.
3. Choose a renderer (`PngByteQRCode`, `SvgQRCode`, ...).
4. Call `GetGraphic(...)` to produce the image bytes/string.
5. Save the result to a file or stream.

# 4. API Reference

## QRCodeGenerator Class

Main class that encodes text/payloads into QR code data.

### Constructor

```csharp
using var generator = new QRCodeGenerator();
```
Implements `IDisposable`; wrap in `using`.

### CreateQrCode(string, ECCLevel)

Encodes plain text into a QR code matrix.

**Syntax**
```csharp
QRCodeData CreateQrCode(string plainText, QRCodeGenerator.ECCLevel eccLevel);
```
**Parameters**

| Name | Type | Description |
|------|------|-------------|
| plainText | string | The text/URL/payload to encode |
| eccLevel | ECCLevel | Error-correction level: L, M, Q, or H |

**Return Value**

| Type | Description |
|------|-------------|
| QRCodeData | The QR module matrix, passed to a renderer |

**Exceptions**

| Exception | When |
|-----------|------|
| DataTooLongException | The payload is too large for a QR code at the chosen ECC level |

**Example**
```csharp
using var generator = new QRCodeGenerator();
using var data = generator.CreateQrCode("Hello World", QRCodeGenerator.ECCLevel.Q);
```

### GenerateQrCode(string, ECCLevel) — static

Static convenience equivalent to constructing a generator and calling `CreateQrCode`.

**Syntax**
```csharp
static QRCodeData GenerateQrCode(string plainText, QRCodeGenerator.ECCLevel eccLevel);
```
**Example**
```csharp
using var data = QRCodeGenerator.GenerateQrCode("Hello World", QRCodeGenerator.ECCLevel.Q);
```

### ECCLevel (enum)

`L` (~7%), `M` (~15%), `Q` (~25%), `H` (~30%) recoverable data. Higher = more robust, larger code.

## PayloadGenerator.OneTimePassword Class

Builds an `otpauth://` string for authenticator apps.

**Key properties**

| Property | Type | Description |
|----------|------|-------------|
| Type | OneTimePasswordAuthType | TOTP (time-based) or HOTP (counter-based) |
| Secret | string | Base32-encoded shared secret |
| Issuer | string | Service/company name |
| Label | string | Account label (e.g. email) |
| Digits | int | Code length (default 6) |
| Period | int? | TOTP period in seconds (default 30) |
| Counter | int? | HOTP counter |
| AuthAlgorithm | OneTimePasswordAuthAlgorithm | SHA1 (default), SHA256, SHA512 |

### ToString()

Returns the `otpauth://` payload string.

**Exceptions**

| Exception | When |
|-----------|------|
| InvalidOperationException | Secret is empty, or Issuer/Label contains ':' |

**Example**
```csharp
var otp = new PayloadGenerator.OneTimePassword
{
    Secret = "JBSWY3DPEHPK3PXP",
    Issuer = "MyApp",
    Label  = "alice@example.com",
};
string payload = otp.ToString();
```

## Renderers

All renderers take a `QRCodeData` in their constructor and expose `GetGraphic(int pixelsPerModule)`.

| Renderer | GetGraphic returns | Platform |
|----------|--------------------|----------|
| PngByteQRCode | byte[] (PNG) | any |
| SvgQRCode | string (SVG) | any |
| BitmapByteQRCode | byte[] (BMP) | any |
| PdfByteQRCode | byte[] (PDF) | any |
| QRCode | System.Drawing.Bitmap | Windows |

**Example (PNG)**
```csharp
using var data = QRCodeGenerator.GenerateQrCode("Hello World", QRCodeGenerator.ECCLevel.Q);
byte[] png = new PngByteQRCode(data).GetGraphic(20);   // 20 px per module
```

## QRCodeData Class

Holds the QR module matrix (`List<BitArray> ModuleMatrix`). `true` = dark module. Implements
`IDisposable`. Access it directly for custom rendering/analysis.

# 5. Complete Example

```csharp
using System.IO;
using QRCoder;

class Program
{
    static void Main()
    {
        var otp = new PayloadGenerator.OneTimePassword
        {
            Secret = "JBSWY3DPEHPK3PXP",
            Issuer = "MyApp",
            Label  = "alice@example.com",
        };

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(otp.ToString(), QRCodeGenerator.ECCLevel.Q);

        byte[] png = new PngByteQRCode(data).GetGraphic(20);
        File.WriteAllBytes("otp.png", png);
    }
}
```

The bundled **QRCoderSamples** app also provides `OtpQrGenerator.CreateTotpQr(secret, issuer,
label, format)` as a one-call convenience wrapper over the above.

# 6. Error Handling

Wrap generation in try/catch and surface the message:

```csharp
try
{
    using var generator = new QRCodeGenerator();
    using var data = generator.CreateQrCode(otp.ToString(), QRCodeGenerator.ECCLevel.Q);
    byte[] png = new PngByteQRCode(data).GetGraphic(20);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

# 7. Exceptions

QRCoder reports failures with typed exceptions (not numeric error codes):

| Exception | Description |
|-----------|-------------|
| DataTooLongException | Payload too large for the QR version/ECC level |
| InvalidOperationException | Invalid OneTimePassword input (empty secret; ':' in issuer/label) |
| ArgumentException | Invalid arguments (e.g. grid given 0 or >4 accounts, in the sample) |
| NotSupportedException | Unsupported format for an operation (e.g. SVG/PDF for the bitmap grid) |
| PlatformNotSupportedException | `QRCode` renderer used on a non-Windows platform |

# 8. Best Practices

## Recommended
```csharp
using var generator = new QRCodeGenerator();
using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
```
Dispose generator/data (`using`). Prefer **PNG** for scannable output. Keep OTP secrets secure
(serve over HTTPS, show once).

## Not Recommended
```csharp
var generator = new QRCodeGenerator();   // never disposed
```
Avoid JPEG for QR (lossy edges hurt scanning). Don't log full otpauth payloads (they contain the secret).

# 9. Version History

| Version | Date | Description |
|---------|------|-------------|
| 1.0.0 | 2026-08-06 | Initial release of the trimmed QRCoder fork |

# 10. Support

- Repository: https://github.com/eunjoonhong07/QR-Code-Test
- Library: QRCoder.dll (trimmed fork; originally by Raffael Herrmann, MIT licensed)
