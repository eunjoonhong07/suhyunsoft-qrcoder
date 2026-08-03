# QRCoder — Function Tree Map

A hierarchical reference of the functions involved in generating a QR code, as wired in this
(trimmed) build: the **QRCoderSamples** app calls the **QRCoder** DLL. Read top-to-bottom;
indentation = "calls". Reflects the current codebase (OTP payload only; 5 renderers).

Legend:  `→` calls / delegates to   ·   `{A | B}` one branch taken   ·   `[loop]` repeated

---

## Layer 0 — End-to-end flow (the one-line summary)

```
CreateTotpQr(secret, issuer, label, format)
   → OneTimePassword.ToString()                → builds "otpauth://..." string
   → QRCodeGenerator.CreateQrCode(text, ECC)   → encodes → QRCodeData (module matrix)
   → <renderer>.GetGraphic(pixelsPerModule)    → image bytes (PNG/SVG/BMP/PDF/JPEG)
```

---

## Layer 1 — Application (QRCoderSamples.exe)

```
Program (top-level statements)                       [Program.cs]
├─ Check(name, condition)                            local test-assert
├─ OtpQrGenerator.CreateTotpQr(...)   → Render       single QR, TOTP
├─ OtpQrGenerator.CreateHotpQr(...)   → Render       single QR, HOTP
├─ OtpQrGenerator.CreateTotpGrid(accounts, ...)      2x2 grid of QRs
│    ├─ [loop] QRCode.GetGraphic(ppm)                one Bitmap per account
│    ├─ ComposeGrid(tiles, ppm)                      tile onto one canvas
│    └─ EncodeBitmap(composite, format)              → PNG/JPEG/BMP bytes
└─ OtpQrGenerator.FileExtension(format)              ".png" / ".svg" / ...
```

### OtpQrGenerator internals (QRCoderSamples/OtpQrGenerator.cs)

```
CreateTotpQr / CreateHotpQr
   → new PayloadGenerator.OneTimePassword { ... } → .ToString()
   → Render(payload, format, ppm)

Render(payload, format, ppm)
   → QRCodeGenerator.CreateQrCode(payload, ECCLevel.Q)
   → switch(format):
        Png  → new PngByteQRCode(data).GetGraphic(ppm)
        Bmp  → new BitmapByteQRCode(data).GetGraphic(ppm)
        Pdf  → new PdfByteQRCode(data).GetGraphic(ppm)
        Svg  → new SvgQRCode(data).GetGraphic(ppm)  → UTF-8 bytes
        Jpeg → RenderJpeg(data, ppm)

RenderJpeg(data, ppm)  → new QRCode(data).GetGraphic(ppm) → Bitmap.Save(stream, Jpeg)

CreateTotpGrid(accounts, format, ppm)
   → [loop] OneTimePassword.ToString() → CreateQrCode → QRCode.GetGraphic(ppm)
   → ComposeGrid(tiles, ppm)   → Graphics.DrawImage per tile
   → EncodeBitmap(composite, format) → Bitmap.Save(...)
```

---

## Layer 2 — Payload (QRCoder DLL: PayloadGenerator.OneTimePassword)

```
OneTimePassword.ToString()
   → switch(Type):  TOTP → TimeToString()   |   HOTP → HMACToString()
TimeToString()  → ProcessCommonFields(sb)   (+ period if != 30)
HMACToString()  → ProcessCommonFields(sb)   (+ counter)
ProcessCommonFields(sb)   builds label/secret/issuer/algorithm/digits
```

---

## Layer 3 — Generation engine (QRCoder DLL: QRCodeGenerator)

```
CreateQrCode(string, ECCLevel)                 [instance, used by sample]
   → GenerateQrCode(text, ecc, forceUtf8=false, utf8BOM=false, eci=Default, version=-1)

GenerateQrCode(string, ECCLevel, ...)          [static — the pipeline]
├─ ValidateECCLevel(eccLevel)
├─ CreateDataSegment(text, forceUtf8, utf8BOM, eci)
│    ├─ OptimizedLatin1DataSegment.CanEncode(text) → new OptimizedLatin1DataSegment  (fast path for otpauth)
│    └─ else GetEncodingFromPlaintext(text) → { NumericDataSegment | AlphanumericDataSegment | ByteDataSegment }
├─ DetermineVersion(segment, ecc, version) → CapacityTables.TryCalculateMinimumVersion (else DataTooLongException)
├─ segment.ToBitArray(version)
└─ GenerateQrCode(BitArray, ecc, version)      [static — ECC + placement]
   ├─ CapacityTables.GetEccInfo(version, ecc)
   ├─ PadData()
   ├─ CalculateECCBlocks()
   │    ├─ CalculateGeneratorPolynom(eccPerBlock)   → Polynom / GaloisField
   │    └─ [loop] AddCodeWordBlocks → CalculateECCWords(...) → CodewordBlock
   ├─ CalculateInterleavedLength()
   ├─ InterleaveData()
   └─ PlaceModules()
        ├─ ModulePlacer.PlaceFinderPatterns
        ├─ ModulePlacer.ReserveSeperatorAreas
        ├─ ModulePlacer.PlaceAlignmentPatterns (AlignmentPatterns.FromVersion)
        ├─ ModulePlacer.PlaceTimingPatterns
        ├─ ModulePlacer.PlaceDarkModule
        ├─ ModulePlacer.ReserveVersionAreas
        ├─ ModulePlacer.PlaceDataWords
        ├─ ModulePlacer.MaskCode        → best mask pattern
        ├─ ModulePlacer.PlaceFormat
        └─ ModulePlacer.PlaceVersion    (v7+)
   → returns QRCodeData
```

Supporting types on this path: `Polynom`, `PolynomItem`, `GaloisField`, `ECCInfo`,
`CodewordBlock`, `CapacityTables`, `VersionInfo`/`VersionInfoDetails`, `AlignmentPattern(s)`,
`Point`, `Rectangle`, `EncodingMode`, `EciMode`.

---

## Layer 4 — Rendering (QRCoder DLL: 5 renderers, all : AbstractQRCode)

Each renderer is constructed with the `QRCodeData`; the sample calls the 1-arg
`GetGraphic(pixelsPerModule)`, which delegates to that renderer's fuller implementation:

```
PngByteQRCode.GetGraphic(ppm, quietZones=true)
   → DrawScanlines(ppm, quietZones) → PngBuilder.WriteHeader/WriteScanlines/WriteEnd → PNG byte[]
BitmapByteQRCode.GetGraphic(ppm)   → GetGraphic(ppm, byte[] dark, byte[] light)       → BMP byte[]
PdfByteQRCode.GetGraphic(ppm)      → GetGraphic(ppm, darkHex, lightHex, dpi, jpgQual)  → PDF byte[]
SvgQRCode.GetGraphic(ppm)          → GetGraphic(Size, darkHex, lightHex, ...) master   → SVG string
QRCode.GetGraphic(ppm)             → GetGraphic(ppm, Color, Color, ...) master
                                        └─ CreatePathFromModules(...) → System.Drawing.Bitmap
```

---

## Notes

- **Scope:** this map covers the path actually exercised by the sample. The DLL also exposes
  public entry points not on that path — `GenerateMicroQrCode`, `CreateQrCode(Payload)`,
  `CreateQrCode(byte[])`, and color/hex `GetGraphic` overloads — which exist but are not called
  by QRCoderSamples.
- **Encoding branch:** an `otpauth://` string is Latin-1, so `CreateDataSegment` always takes the
  `OptimizedLatin1DataSegment` fast path; the standalone `Numeric/Alphanumeric/Byte` segment
  classes are reached only for other inputs.
- **Format:** authored in Markdown for easy printing; can be exported to PDF/Word (e.g. via a
  Markdown viewer or `pandoc`) for the paper reference.
