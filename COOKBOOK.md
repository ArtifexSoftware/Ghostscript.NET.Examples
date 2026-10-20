# Ghostscript.NET cookbook

Task-oriented C# recipes for [Ghostscript.NET](https://www.nuget.org/packages/Ghostscript.NET/).
Each recipe has a matching console project in this solution (NuGet-only; no project reference to Ghostscript.NET source).

Requires a Ghostscript native library (`gsdll64.dll` / `libgs.so`), either from `Ghostscript.NativeAssets` or a system install. Office recipes also need GhostPDL (`gpdldll64.dll` / `libgpdl.so`).

```xml
<PackageReference Include="Ghostscript.NET" Version="1.3.6" />
<PackageReference Include="Ghostscript.NativeAssets" Version="10.8.0" />
```

Or the convenience package [Ghostscript.NET.Office](https://www.nuget.org/packages/Ghostscript.NET.Office/) (1.0.0), which depends on both of the above. You still call `GhostscriptOffice.Unlock(key)` for the full document.

Page numbers are **1-based**. Prefer `GhostscriptVersionInfo.GetPreferredVersion()` (NativeAssets, then system install).

---

## What it can do

| Task | Class |
|------|--------|
| Rasterize PDF / PS / EPS to a bitmap | `GhostscriptRasterizer` |
| Write PNG / JPEG (and other GS devices) to disk | `GhostscriptPngDevice`, `GhostscriptJpegDevice`, `GhostscriptProcessor` |
| Convert / compress PDF, PS → PDF | `GhostscriptProcessor` (`pdfwrite`) |
| Office (Word / Excel / PowerPoint / Hangul / …) → PDF | `GhostscriptOffice` (GhostPDL) |
| Rasterize an Office file | `GhostscriptRasterizer.Open(officePath)` or `GhostscriptProcessor` |
| Watermark | `GhostscriptProcessor` + PostScript `EndPage` |
| Print (Windows) | `GhostscriptProcessor` + `mswinpr2` |
| Capture stdout / write to a pipe | `GhostscriptStdIO`, `GhostscriptPipedOutput` |
| Run two Ghostscript jobs at once | two `GhostscriptProcessor` instances |
| CMYK ink coverage | `GhostscriptPdfInfo.GetInkCoverage` |
| Interactive view (zoom) | `GhostscriptViewer` (see `Ghostscript.NET.Viewer`) |
| PDF/A-3 + ZUGFeRD / Factur-X | `PDFA3Converter` |

GhostPDL allows **one interpreter instance per process**. Without an Office unlock key, conversion is limited to the first **3 pages**. Set `GHOSTSCRIPT_NET_OFFICE_KEY` (do not hard-code keys).

---

## 1. Rasterize every PDF page to PNG

```csharp
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using SkiaSharp;

var gs = GhostscriptVersionInfo.GetPreferredVersion();

using var rasterizer = new GhostscriptRasterizer();
rasterizer.Open("input.pdf", gs, dllFromMemory: false);

for (int page = 1; page <= rasterizer.PageCount; page++)
{
    SKBitmap image = rasterizer.GetPage(dpi: 150, pageNumber: page);
    using SKImage sk = SKImage.FromBitmap(image);
    using SKData data = sk.Encode(SKEncodedImageFormat.Png, 100);
    using FileStream stream = File.OpenWrite($"page_{page:D3}.png");
    data.SaveTo(stream);
}
```

Runnable: `01-RasterizePng`.

---

## 2. Rasterize from a stream (no extra temp input file)

```csharp
using var stream = File.OpenRead("input.pdf");
using var rasterizer = new GhostscriptRasterizer();
rasterizer.Open(stream, GhostscriptVersionInfo.GetPreferredVersion(), dllFromMemory: false);

SKBitmap page1 = rasterizer.GetPage(dpi: 150, pageNumber: 1);
```

Runnable: `02-RasterizeStream`.

---

## 3. Anti-aliasing and CropBox

CropBox is in PDF points (origin bottom-left): `[llx lly urx ury]`. The rectangle must sit inside the page MediaBox. Write it onto the PDF with `pdfmark` (include `-dUseCropBox` on `pdfwrite` or Ghostscript discards it), then rasterize with `-dUseCropBox`.

```csharp
const string cropBox = "[/CropBox [24 72 500 360] /PAGES pdfmark";

using (var processor = new GhostscriptProcessor())
{
    processor.StartProcessing(new[]
    {
        "",
        "-dBATCH", "-dNOPAUSE",
        "-dUseCropBox",
        "-sDEVICE=pdfwrite",
        "-sOutputFile=cropped.pdf",
        "-c", cropBox,
        "-f", "input.pdf"
    }, stdIO_callback: null);
}

using var rasterizer = new GhostscriptRasterizer();
rasterizer.GraphicsAlphaBits = 4;
rasterizer.TextAlphaBits = 4;
rasterizer.CustomSwitches.Add("-dUseCropBox");
rasterizer.Open("cropped.pdf", GhostscriptVersionInfo.GetPreferredVersion(), false);

SKBitmap page = rasterizer.GetPage(dpi: 300, pageNumber: 1);
```

For EPS bounding-box clip, set `rasterizer.EPSClip = true`. Runnable: `03-AntiAliasCropBox`.

---

## 4. PDF pages to PNG with `GhostscriptPngDevice`

```csharp
using Ghostscript.NET;

var device = new GhostscriptPngDevice(GhostscriptPngDeviceType.Png16m);
device.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
device.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
device.ResolutionXY = new GhostscriptImageDeviceResolution(300, 300);
device.InputFiles.Add("input.pdf");
device.Pdf.FirstPage = 1;
device.Pdf.LastPage = 5;
device.OutputPath = @"output\page_%03d.png";
device.Process();
```

Grayscale: `GhostscriptPngDeviceType.PngGray`. Runnable: `04-PngDevice`.

---

## 5. PDF pages to JPEG

```csharp
using Ghostscript.NET;

var device = new GhostscriptJpegDevice(GhostscriptJpegDeviceType.Jpeg);
device.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
device.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
device.ResolutionXY = new GhostscriptImageDeviceResolution(150, 150);
device.JpegQuality = 85;
device.InputFiles.Add("input.pdf");
device.OutputPath = @"output\page_%03d.jpg";
device.Process();
```

Runnable: `05-JpegDevice`.

---

## 6. Compress a PDF (`pdfwrite`)

```csharp
using Ghostscript.NET.Processor;

using var processor = new GhostscriptProcessor();
processor.Processing += (_, e) =>
    Console.WriteLine($"Page {e.CurrentPage} / {e.TotalPages}");

processor.Process(new[]
{
    "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
    "-sDEVICE=pdfwrite",
    "-dPDFSETTINGS=/ebook",
    "-sOutputFile=compressed.pdf",
    "input.pdf"
});

Console.WriteLine($"Before: {new FileInfo("input.pdf").Length:N0} bytes");
Console.WriteLine($"After:  {new FileInfo("compressed.pdf").Length:N0} bytes");
```

Runnable: `06-CompressPdf`.

---

## 7. PostScript / EPS → PDF

```csharp
using Ghostscript.NET.Processor;

using var processor = new GhostscriptProcessor();
processor.Process(new[]
{
    "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
    "-sDEVICE=pdfwrite",
    "-sOutputFile=output.pdf",
    "-f",
    "input.ps"
});
```

Runnable: `07-PsToPdf`.

---

## 8. Page range with `GhostscriptProcessor`

`-dFirstPage` / `-dLastPage` are 1-based. `%03d` in the output path is the output sequence (1, 2, 3, …), not the PDF page number. The runnable example uses pages 2–4 of a 12-page PDF and prints the mapping.

```csharp
using Ghostscript.NET.Processor;

using var processor = new GhostscriptProcessor();
processor.Process(new[]
{
    "-dSAFER", "-dBATCH", "-dNOPAUSE",
    "-dFirstPage=2",
    "-dLastPage=4",
    "-sDEVICE=png16m",
    "-r96",
    "-dTextAlphaBits=4",
    "-dGraphicsAlphaBits=4",
    "-sOutputFile=page-%03d.png",
    "-f",
    "input.pdf"
});
```

Runnable: `08-PageRange`.

---

## 9. Watermark a PDF

`pdfmark /ca` is **not** enough: `pdfwrite` ignores it and the stamp stays opaque. Real PDF transparency needs `-dALLOWPSTRANSPARENCY` (or `.setfillconstantalpha` is undefined) and the pdf14 compositor. `0.25 .setfillconstantalpha` is 25% opaque gray; page text shows through the letters.

```csharp
using Ghostscript.NET.Processor;

const string watermark = @"
<< /PageUsesTransparency true /CompatibilityLevel 1.4 >> setpagedevice
0 .pushpdf14devicefilter
/watermarkText { (CONFIDENTIAL) } def
/watermarkFont { /Helvetica-Bold 72 selectfont } def
/watermarkAngle { 45 } def
/pageWidth { currentpagedevice /PageSize get 0 get } def
/pageHeight { currentpagedevice /PageSize get 1 get } def
<< /EndPage {
    2 eq { pop false } {
        gsave
        0.25 .setfillconstantalpha
        watermarkFont
        .75 setgray
        pageWidth .5 mul pageHeight .5 mul translate
        0 0 moveto watermarkText false charpath flattenpath pathbbox
        4 2 roll pop pop 0 0 moveto watermarkAngle rotate
        -.5 mul exch -.5 mul exch rmoveto
        watermarkText show
        grestore true
    } ifelse
} bind >> setpagedevice";

using var processor = new GhostscriptProcessor();
processor.StartProcessing(new[]
{
    "",
    "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
    "-dALLOWPSTRANSPARENCY",
    "-dCompatibilityLevel=1.4",
    "-sDEVICE=pdfwrite",
    "-sOutputFile=watermarked.pdf",
    "-c", watermark,
    "-f", "input.pdf"
}, stdIO_callback: null);
```

Runnable: `09-Watermark` (`ink.pdf`).

---

## 10. Word / Excel / PowerPoint / Hangul → PDF (GhostPDL)

```csharp
using Ghostscript.NET;

if (!GhostscriptVersionInfo.TryGetGhostPdlVersion(out GhostscriptVersionInfo pdl))
    throw new InvalidOperationException("GhostPDL not found (gpdldll / libgpdl).");

string? key = Environment.GetEnvironmentVariable("GHOSTSCRIPT_NET_OFFICE_KEY");
if (!string.IsNullOrWhiteSpace(key))
    GhostscriptOffice.Unlock(key);

GhostscriptOffice.ConvertToPdf(@"D:\report.docx", @"D:\report.pdf", pdl);
```

Supported extensions include `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.odt`, `.ods`, `.odp`, `.rtf`, `.csv`, `.hwp`, `.hwpx`. Use **full paths**.

Or pass an Office path to `GhostscriptProcessor` (any Ghostscript device). GhostPDL is loaded automatically; `CreateForInput` picks it up front:

```csharp
using Ghostscript.NET.Processor;

using var processor = GhostscriptProcessor.CreateForInput(@"D:\report.docx");
processor.Process(new[]
{
    "-dBATCH", "-dNOPAUSE",
    "-sDEVICE=png16m",
    "-sOutputFile=page-%03d.png",
    @"D:\report.docx"
});
```

Rasterize Office the same way as PDF:

```csharp
using var rasterizer = new GhostscriptRasterizer();
rasterizer.Open(@"D:\report.docx");
SKBitmap page1 = rasterizer.GetPage(dpi: 150, pageNumber: 1);
```

Runnable: `10-OfficeToPdf`. The convenience package `Ghostscript.NET.Office` 1.0.0 pulls Ghostscript.NET + NativeAssets (including GhostPDL).

---

## 11. Print on Windows

```csharp
using Ghostscript.NET.Processor;

using var processor = new GhostscriptProcessor();
processor.StartProcessing(new[]
{
    "",
    "-dPrinted", "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
    "-dNumCopies=1",
    "-sDEVICE=mswinpr2",
    "-sOutputFile=%printer%Microsoft Print to PDF",
    "-f",
    "input.pdf"
}, stdIO_callback: null);
```

`mswinpr2` can show a Save dialog and block. The example only runs when `GHOSTSCRIPT_NET_RUN_PRINTER=1`. Runnable: `11-Print`.

---

## 12. Capture Ghostscript stdout / stderr

```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;

sealed class ConsoleStdIO : GhostscriptStdIO
{
    public ConsoleStdIO() : base(true, true, true) { }

    public override void StdIn(out string input, int count) => input = string.Empty;
    public override void StdOut(string output) => Console.Write(output);
    public override void StdError(string error) => Console.Error.Write(error);
}

using var processor = new GhostscriptProcessor();
processor.Error += (_, e) => Console.Error.WriteLine(e.Message);
processor.Process(new[]
{
    "-dBATCH", "-dNOPAUSE",
    "-sDEVICE=pdfwrite",
    "-sOutputFile=out.pdf",
    "input.pdf"
}, new ConsoleStdIO());
```

Runnable: `12-CaptureStdio`.

---

## 13. Write device output to a pipe (no output file)

Use `%handle%` so Ghostscript writes into memory:

```csharp
using Ghostscript.NET.Processor;

var pipe = new GhostscriptPipedOutput();
string handle = "%handle%" + int.Parse(pipe.ClientHandle).ToString("X2");

using var processor = new GhostscriptProcessor();
processor.Process(new[]
{
    "-dQUIET", "-dSAFER", "-dBATCH", "-dNOPAUSE", "-dNOPROMPT",
    "-sDEVICE=pdfwrite",
    "-sOutputFile=" + handle,
    "-f", "input.ps"
});

byte[] pdfBytes = pipe.Data;
```

Runnable: `13-PipedOutput`.

---

## 14. Two jobs in one process

Ghostscript (not GhostPDL) can run multiple instances. Start two `GhostscriptProcessor` jobs and wait for both. On Windows, load the native library from memory so the two instances do not share one DLL mapping.

GhostPDL Office conversion: **one instance per process** only.

Runnable: `14-MultipleInstances`.

---

## 15. CMYK ink coverage

```csharp
using Ghostscript.NET;

Dictionary<int, GhostscriptPageInkCoverage> pages =
    GhostscriptPdfInfo.GetInkCoverage("input.pdf");

foreach (KeyValuePair<int, GhostscriptPageInkCoverage> kv in pages)
{
    GhostscriptPageInkCoverage c = kv.Value;
    Console.WriteLine(
        $"Page {kv.Key} C:{c.C}% M:{c.M}% Y:{c.Y}% K:{c.K}%");
}
```

There is also a page-from / page-to overload. Runnable: `15-InkCoverage`.

---

## 16. Point at a specific Ghostscript DLL

```csharp
var gs = new GhostscriptVersionInfo(@"C:\gs\gs10.08.0\bin\gsdll64.dll");

using var rasterizer = new GhostscriptRasterizer();
rasterizer.Open("input.pdf", gs, dllFromMemory: false);
```

Windows-only: load the DLL from a `byte[]` with `rasterizer.Open("input.pdf", dllBytes)`. On Linux/macOS load from disk. Runnable: `16-CustomDllPath`.

---

## 17. PDF/A-3 and ZUGFeRD / Factur-X

`ConvertToPDFA3` currently requires an embedded XML invoice (Factur-X / ZUGFeRD). `SetZUGFeRDProfile` takes a string (for example `"EN 16931"`).

```csharp
using Ghostscript.NET;

var converter = new Ghostscript.NET.PDFA3Converter.PDFA3Converter(
    GhostscriptVersionInfo.GetPreferredVersion().DllPath);
converter.SetZUGFeRDProfile("EN 16931");
converter.SetZUGFeRDVersion("2.3");
converter.SetEmbeddedXMLFile("factur-x.xml");
converter.ConvertToPDFA3("invoice.pdf", "invoice-pdfa3-zugferd.pdf");
```

Runnable: `17-PdfA3`. For generating the XML, see also `Ghostscript.NET.PDFA3Converter.Samples`.

---

## 18. Interactive viewer

Use `GhostscriptViewer` (zoom, progressive page updates). Desktop sample: `Ghostscript.NET.Viewer`. Display-device test: `Ghostscript.NET.DisplayTest`.

---

## Sample project map

Customer-facing console apps in this solution (NuGet packages only):

| Recipe | Project |
|--------|---------|
| 1 | `01-RasterizePng` |
| 2 | `02-RasterizeStream` |
| 3 | `03-AntiAliasCropBox` |
| 4 | `04-PngDevice` |
| 5 | `05-JpegDevice` |
| 6 | `06-CompressPdf` |
| 7 | `07-PsToPdf` |
| 8 | `08-PageRange` |
| 9 | `09-Watermark` |
| 10 | `10-OfficeToPdf` |
| 11 | `11-Print` |
| 12 | `12-CaptureStdio` |
| 13 | `13-PipedOutput` |
| 14 | `14-MultipleInstances` |
| 15 | `15-InkCoverage` |
| 16 | `16-CustomDllPath` |
| 17 | `17-PdfA3` |
| 18 | `Ghostscript.NET.Viewer` (WinForms; not a console example) |

```powershell
dotnet restore
dotnet run --project 01-RasterizePng
```

Library-development samples live in [Ghostscript.NET](https://github.com/ArtifexSoftware/Ghostscript.NET) under `Ghostscript.NET.Samples/` (they project-reference Ghostscript.NET source).
