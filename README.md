# QRCoder

[![License](https://img.shields.io/github/license/Shane32/QRCoder)](LICENSE.txt)

QRCoder is a simple C# library originally created by [Raffael Herrmann](https://raffaelherrmann.de) for generating QR codes and Micro QR codes.

> **Note:** This repository is a trimmed, customized build of QRCoder. The solution contains just two projects — the **QRCoder** class library (single-target **.NET 8.0**, producing one reusable `QRCoder.dll`) and **QRCoderSamples**, a sample console app that demonstrates the library (including a custom one-time-password QR generator). The upstream project targets many more frameworks and ships additional packages; the wiki links below point to that upstream documentation.

- 📚 [Documentation & Wiki](https://github.com/Shane32/QRCoder/wiki)
- 📋 [Release notes / Changelog](https://github.com/Shane32/QRCoder/releases)
- 🚀 [Upcoming features](https://github.com/Shane32/QRCoder/milestones)

## ✨ Features

- 🪶 **Minimal dependencies** - Only `System.Drawing.Common` (used by the bitmap-based renderers)
- ⚡ **Fast performance** - Optimized QR code generation with low memory footprint
- 🎨 **Multiple output formats** - PNG, SVG, PDF, ASCII, Bitmap, PostScript, and more
- 📱 **23+ payload generators** - WiFi, vCard, URLs, payments, and many more
- 🔧 **Highly configurable** - Error correction levels, custom colors, logos, and styling
- 🌐 **Modern .NET** - Targets **.NET 8.0**; consumable by any .NET 8+ application
- 📦 **Micro QR codes** - Smaller QR codes for space-constrained applications

## 📦 Building from source

This fork isn't published to NuGet — you build the DLL directly from source. Requires the .NET SDK (8.0 or newer).

```bash
# Build the single reusable library DLL:
dotnet build QRCoder/QRCoder.csproj -c Release
#   -> QRCoder/bin/Release/net8.0/QRCoder.dll

# Build and run the sample application:
dotnet run --project QRCoderSamples -c Release
```

To use the DLL in your own project, add a project reference to `QRCoder/QRCoder.csproj`, or reference the compiled `QRCoder.dll` directly.

## 🗂️ Repository structure

The solution (`QRCoder.sln`) contains exactly two projects:

| Project | Type | Output | Description |
|---------|------|--------|-------------|
| **QRCoder** | Class library (`net8.0`) | `QRCoder.dll` | The reusable QR-code library. |
| **QRCoderSamples** | Console app (`net10.0-windows`) | `QRCoderSamples.exe` | Sample app that references the DLL and demonstrates its use. |

Inside `QRCoder/`, the source is organized by concern:

```
QRCoder/
├── QRCodeData.cs            # core module-matrix data model
├── Rendering/               # all output renderers (PNG, SVG, PDF, BMP, ASCII, ...)
├── QRCodeGenerator/         # QR encoding engine (partial class + fragments)
├── PayloadGenerator/        # payload builders (WiFi, URL, OneTimePassword, ...)
└── Attributes/  Exceptions/  Extensions/
```

## 🚀 Quick Start

Generate a QR code with just a few lines of code, either using a renderer's static helper method, or by creating a QR code first and then passing it to a renderer:

```csharp
using QRCoder;

// Generate a simple black and white PNG QR code
byte[] qrCodeImage = PngByteQRCodeHelper.GetQRCode("Hello World", QRCodeGenerator.ECCLevel.Q, 20);

// Generate a scalable black and white SVG QR code
using var qrCodeData = QRCodeGenerator.GenerateQrCode("Hello World", QRCodeGenerator.ECCLevel.Q);
using var svgRenderer = new SvgQRCode(qrCodeData);
string svg = svgRenderer.GetGraphic();
```

For more examples and detailed usage instructions, see: [Wiki: How to use QRCoder](https://github.com/Shane32/QRCoder/wiki/How-to-use-QRCoder)

## 📱 Payload Generators

QR codes can encode structured data that triggers specific actions when scanned (e.g., WiFi credentials, contact information, URLs). QRCoder includes payload generators that create properly formatted strings for these common use cases.

### Usage Example

```csharp
using QRCoder;

// Create a bookmark payload
var bookmarkPayload = new PayloadGenerator.Bookmark("https://github.com/Shane32/QRCoder", "QRCoder Repository");

// Generate the QR code data from the payload
using var qrCodeData = QRCodeGenerator.GenerateQrCode(bookmarkPayload);

// Or override the ECC level
using var qrCodeData2 = QRCodeGenerator.GenerateQrCode(bookmarkPayload, QRCodeGenerator.ECCLevel.H);

// Render the QR code
using var pngRenderer = new PngByteQRCode(qrCodeData);
byte[] qrCodeImage = pngRenderer.GetGraphic(20);
```

### Available Payload Types

| Payload Type | Usage Example | Description |
|--------------|---------------|-------------|
| [**WiFi**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#321-wifi) | `new PayloadGenerator.WiFi(ssid, password, auth)` | WiFi network credentials |
| [**URL**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#319-url) | `new PayloadGenerator.Url("https://example.com")` | Website URL |
| [**Bookmark**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#33-bookmark) | `new PayloadGenerator.Bookmark(url, title)` | Browser bookmark |
| [**Mail**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#38-mail) | `new PayloadGenerator.Mail(email, subject, body)` | Email with pre-filled fields |
| [**SMS**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#317-sms) | `new PayloadGenerator.SMS(number, message)` | SMS message |
| [**MMS**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#39-mms) | `new PayloadGenerator.MMS(number, subject)` | MMS message |
| [**Geolocation**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#36-geolocation) | `new PayloadGenerator.Geolocation(lat, lng)` | GPS coordinates |
| [**PhoneNumber**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#312-phonenumber) | `new PayloadGenerator.PhoneNumber(number)` | Phone number for calling |
| [**SkypeCall**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#315-skype-call) | `new PayloadGenerator.SkypeCall(username)` | Skype call |
| [**WhatsAppMessage**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#320-whatsappmessage) | `new PayloadGenerator.WhatsAppMessage(number, msg)` | WhatsApp message |
| [**ContactData**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#35-contactdata-mecardvcard) | `new PayloadGenerator.ContactData(...)` | vCard/MeCard contact |
| [**CalendarEvent**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#34-calendar-events-icalvevent) | `new PayloadGenerator.CalendarEvent(...)` | iCal/vEvent |
| [**OneTimePassword**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#311-one-time-password) | `new PayloadGenerator.OneTimePassword(...)` | TOTP/HOTP for 2FA |
| [**BitcoinAddress**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#32-bitcoin-like-crypto-currency-payment-address) | `new PayloadGenerator.BitcoinAddress(address)` | Bitcoin payment |
| [**BitcoinCashAddress**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#32-bitcoin-like-crypto-currency-payment-address) | `new PayloadGenerator.BitcoinCashAddress(address)` | Bitcoin Cash payment |
| [**LitecoinAddress**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#32-bitcoin-like-crypto-currency-payment-address) | `new PayloadGenerator.LitecoinAddress(address)` | Litecoin payment |
| [**MoneroTransaction**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#310-monero-addresspayment) | `new PayloadGenerator.MoneroTransaction(...)` | Monero payment |
| [**SwissQrCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#318-swissqrcode-iso-20022) | `new PayloadGenerator.SwissQrCode(...)` | Swiss QR bill (ISO-20022) |
| [**Girocode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#37-girocode) | `new PayloadGenerator.Girocode(...)` | SEPA payment (EPC QR) |
| [**BezahlCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#31-bezahlcode) | `new PayloadGenerator.BezahlCode(...)` | German payment code |
| [**RussiaPaymentOrder**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#313-russiapaymentorder) | `new PayloadGenerator.RussiaPaymentOrder(...)` | Russian payment (ГОСТ Р 56042-2014) |
| [**SlovenianUpnQr**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#316-slovenianupnqr) | `new PayloadGenerator.SlovenianUpnQr(...)` | Slovenian UPN payment |
| [**ShadowSocksConfig**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators#314-shadowsocks-configuration) | `new PayloadGenerator.ShadowSocksConfig(...)` | Shadowsocks proxy config |

For detailed information about payload generators, see: [Wiki: Advanced usage - Payload generators](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---Payload-generators)

## 🎨 QR Code Renderers

QRCoder provides multiple renderers for different output formats and use cases. Each renderer has specific capabilities and framework requirements.

| Renderer | Output Format | Requires | Usage Example |
|----------|---------------|----------|---------------|
| [**PngByteQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#25-pngbyteqrcode-renderer-in-detail) | PNG byte array | — | `new PngByteQRCode(data).GetGraphic(20)` |
| [**SvgQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#26-svgqrcode-renderer-in-detail) | SVG string | — | `new SvgQRCode(data).GetGraphic(20)` |
| [**QRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#21-qrcode-renderer-in-detail) | System.Drawing.Bitmap | Windows¹ | `new QRCode(data).GetGraphic(20)` |
| [**ArtQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#211-artqrcode-renderer-in-detail) | Artistic bitmap with custom images | Windows¹ | `new ArtQRCode(data).GetGraphic(20)` |
| [**AsciiQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#22-asciiqrcode-renderer-in-detail) | ASCII art string | — | `new AsciiQRCode(data).GetGraphic(1)` or `new AsciiQRCode(data).GetGraphicSmall()` |
| [**Base64QRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#23-base64qrcode-renderer-in-detail) | Base64 encoded image | — | `new Base64QRCode(data).GetGraphic(20)` |
| [**BitmapByteQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#24-bitmapbyteqrcode-renderer-in-detail) | BMP byte array | — | `new BitmapByteQRCode(data).GetGraphic(20)` |
| [**PdfByteQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#210-pdfbyteqrcode-renderer-in-detail) | PDF byte array | — | `new PdfByteQRCode(data).GetGraphic(20)` |
| [**PostscriptQRCode**](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers#29-postscriptqrcode-renderer-in-detail) | PostScript/EPS string | — | `new PostscriptQRCode(data).GetGraphic(20)` |

**Notes:**
- ¹ Requires Windows or the `System.Drawing.Common` package (uses GDI+)

> The upstream `XamlQRCode` and `UnityQRCode` renderers are **not** included in this fork — the `QRCoder.Xaml` project was removed. Use the cross-platform renderers above.

For comprehensive information about renderers, see: [Wiki: Advanced usage - QR Code renderers](https://github.com/Shane32/QRCoder/wiki/Advanced-usage---QR-Code-renderers)

## 🧪 Sample application (QRCoderSamples)

`QRCoderSamples` is a console app that demonstrates the library. It includes `OtpQrGenerator`, a helper built on QRCoder's `PayloadGenerator.OneTimePassword` that produces two-factor-authentication (TOTP/HOTP) QR codes:

```csharp
using QRCoderSamples;

// A TOTP QR code (scannable by Google Authenticator, Authy, etc.), as PNG bytes:
byte[] png = OtpQrGenerator.CreateTotpQr("JBSWY3DPEHPK3PXP", "MyApp", "alice@example.com");
File.WriteAllBytes("otp.png", png);

// Choose a different image format — PNG, SVG, BMP, PDF, or JPEG:
byte[] svg = OtpQrGenerator.CreateTotpQr("JBSWY3DPEHPK3PXP", "MyApp", "alice@example.com",
                                         format: QrImageFormat.Svg);

// Combine up to four accounts into ONE 2x2 grid image (PNG/JPEG/BMP):
byte[] grid = OtpQrGenerator.CreateTotpGrid(new (string, string, string)[]
{
    ("JBSWY3DPEHPK3PXP", "MyApp", "alice@example.com"),
    ("KRSXG5CTMVRXEZLU", "MyApp", "bob@example.com"),
});
```

Running the sample (`dotnet run --project QRCoderSamples -c Release`) writes example QR files — single codes in every format plus the 2x2 grids — to the app's output folder, and self-checks that each output is a valid image. Note that the OTP helper and the grid/JPEG output use `System.Drawing`, so the sample targets `net10.0-windows`.

## 🔧 Advanced Features

### Micro QR Codes

QRCoder supports Micro QR codes, which are smaller versions of standard QR codes suitable for applications with limited space. Micro QR codes have significantly limited storage capacity—as few as 5 numeric digits (M1) or as many as 35 numeric digits (M4), with alphanumeric and byte data storing considerably less.

```csharp
using QRCoder;

// Generate a Micro QR code (versions M1-M4, represented as -1 to -4)
using var qrCodeData = QRCodeGenerator.GenerateMicroQrCode("Hello", QRCodeGenerator.ECCLevel.L, requestedVersion: -2);
using var qrCode = new PngByteQRCode(qrCodeData);
byte[] qrCodeImage = qrCode.GetGraphic(20);
```

**Note:** Micro QR codes have limitations on data capacity and error correction levels. They support versions M1 through M4 (specified as -1 to -4), and not all ECC levels are available for all versions. M1 only supports detection (no ECC), M2 and M3 support L and M levels, and M4 supports L, M, and Q levels. For detailed capacity tables, see the [Micro QR Code specification](https://www.qrcode.com/en/codes/microqr.html).

### Working with QRCodeData

`QRCodeData` is the core data structure that represents a QR code's module matrix. It contains a `List<BitArray>` called `ModuleMatrix`, where each `BitArray` represents a row of modules in the QR code. A module is set to `true` for dark/black modules and `false` for light/white modules.

You can access the `ModuleMatrix` directly to read or manipulate the QR code data at the module level. This is useful for custom rendering implementations or analyzing QR code structure.

```csharp
using QRCoder;

// Generate QR code data
using var qrCodeData = QRCodeGenerator.GenerateQrCode("Hello World", QRCodeGenerator.ECCLevel.Q);

// Access the module matrix
var moduleMatrix = qrCodeData.ModuleMatrix;
int size = moduleMatrix.Count; // Size of the QR code (includes quiet zone)

// Manually render as ASCII (versus the included ASCII renderer)
for (int y = 0; y < size; y++)
{
    for (int x = 0; x < size; x++)
    {
        // Check if module is dark (true) or light (false)
        bool isDark = moduleMatrix[y][x];
        Console.Write(isDark ? "██" : "  ");
    }
    Console.WriteLine();
}
```

## ⚠️ Troubleshooting

### System.Drawing.Common Warnings (QRCode and ArtQRCode renderers)

The `QRCode` and `ArtQRCode` renderers depend on `System.Drawing.Common`, which Microsoft has [removed cross-platform support for in .NET 6+](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only). You may encounter one of the following build or runtime errors:

```
CA1416: This call site is reachable on all platforms. 'QRCode.QRCode(QRCodeData)' is only supported on: 'windows'

System.TypeInitializationException: The type initializer for 'Gdip' threw an exception.

System.PlatformNotSupportedException: System.Drawing.Common is not supported on this platform.
```

Solutions include:

1. Use Windows-specific TFMs such as `<TargetFramework>net8.0-windows</TargetFramework>`
2. Mark methods with the `[SupportedOSPlatform("windows")]` attribute
3. Add platform guards by wrapping code with `#if WINDOWS` or `if (OperatingSystem.IsWindows())`
4. Use cross-platform renderers such as `PngByteQRCode`, `SvgQRCode`, or `BitmapByteQRCode`

### ISO-8859-2 Encoding Support (.NET Core and .NET 5+)

ISO-8859-2 encoding is not natively supported on .NET Core and .NET 5+. If you need to use ISO-8859-2 encoding in your code, you must:

1. Install the `System.Text.Encoding.CodePages` NuGet package
2. Register the encoding provider in your application startup code:

```csharp
using System.Text;

// Register the code pages encoding provider
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

Note that the `RussiaPaymentOrder` payload generator already includes this registration internally, so no additional setup is required when using that class.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

QRCoder is a project originally by [Raffael Herrmann](https://raffaelherrmann.de) and was first released in 10/2013. It's licensed under the [MIT license](https://github.com/Shane32/QRCoder/blob/master/LICENSE.txt).

Since 2025, QRCoder has been maintained by [Shane32](https://github.com/Shane32) with contributions from the community.

## 🙏 Credits

Glory to Jehovah, Lord of Lords and King of Kings, creator of Heaven and Earth, who through his Son Jesus Christ, has redeemed me to become a child of God. -[Shane32](https://github.com/Shane32)
