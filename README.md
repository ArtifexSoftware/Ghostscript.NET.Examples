# Ghostscript.NET.Examples

Customer-facing console samples for **[Ghostscript.NET](https://www.nuget.org/packages/Ghostscript.NET/)**.
Each project is one cookbook recipe. Packages are referenced via **NuGet only** — this solution does not project-reference Ghostscript.NET source.

Cookbook: [`COOKBOOK.md`](COOKBOOK.md) in this repo.

## Clone and restore

```powershell
git clone https://github.com/ArtifexSoftware/Ghostscript.NET.Examples.git
cd Ghostscript.NET.Examples
dotnet restore
dotnet run --project 01-RasterizePng
```

Package versions are pinned in [`Versions.props`](Versions.props). [`NuGet.Config`](NuGet.Config) uses a repo-local **`LocalNuget/`** folder (optional pre-release packages) and **nuget.org**. On Windows you can add a `NuGet.Config.local` (gitignored) if you pack to `D:\Artifex\LocalNuget`.

## Layout

| Path | Role |
|------|------|
| `Common/` | Shared paths / console helpers |
| `Input/` | Fixtures (committed) |
| `Output/` | Generated files (gitignored) |
| `01`–`17` | One console recipe each |
| `LocalNuget/` | Optional local `.nupkg` drop folder |
| `LICENSE.md` | Artifex Community License |
| `SECURITY.md` | Vulnerability reporting |

## Prerequisites

- .NET 8 SDK (Windows or Linux x64)
- Packages from [nuget.org](https://www.nuget.org/) (see `Versions.props`): `Ghostscript.NET` 1.3.6 and `Ghostscript.NativeAssets` 10.8.0
- Optional `GHOSTSCRIPT_NET_OFFICE_KEY` for full Office documents (without it, GhostPDL converts the first 3 pages)
- Optional `GHOSTSCRIPT_NET_RUN_PRINTER=1` to run `11-Print`
- The convenience package [Ghostscript.NET.Office](https://www.nuget.org/packages/Ghostscript.NET.Office/) 1.0.0 pulls Ghostscript.NET + NativeAssets

Do not commit license keys.

## Example projects

| Project | Recipe |
|---------|--------|
| `01-RasterizePng` | Rasterize every PDF page to PNG |
| `02-RasterizeStream` | Rasterize from a stream |
| `03-AntiAliasCropBox` | Anti-aliasing and CropBox |
| `04-PngDevice` | `GhostscriptPngDevice` |
| `05-JpegDevice` | JPEG pages |
| `06-CompressPdf` | Compress PDF (`pdfwrite`) |
| `07-PsToPdf` | PostScript → PDF |
| `08-PageRange` | PDF pages 2–4 via `-dFirstPage` / `-dLastPage` |
| `09-Watermark` | Transparent gray CONFIDENTIAL overlay (`ink.pdf`) |
| `10-OfficeToPdf` | Word / Hangul → PDF, or Office path in `GhostscriptProcessor` |
| `11-Print` | Windows print (`GHOSTSCRIPT_NET_RUN_PRINTER=1`) |
| `12-CaptureStdio` | Capture stdout/stderr |
| `13-PipedOutput` | Write PDF to a pipe |
| `14-MultipleInstances` | Two Ghostscript jobs |
| `15-InkCoverage` | CMYK ink coverage |
| `16-CustomDllPath` | Explicit `gsdll` path |
| `17-PdfA3` | PDF/A-3 + Factur-X / ZUGFeRD XML |

Inputs: `Input/` (`sample.pdf`, `sample.ps`, `sample.docx`, `watermark.pdf`, `ink.pdf`, `invoice.pdf`, `factur-x.xml`). Outputs: `Output/{project}/` (gitignored).

WinForms viewer is not in this solution; see [Ghostscript.NET.Viewer](https://github.com/ArtifexSoftware/Ghostscript.NET).

## Versions

Edit `Versions.props` only.

## License

See [`LICENSE.md`](LICENSE.md) (Artifex Community License). The NuGet packages these samples use (`Ghostscript.NET`, `Ghostscript.NativeAssets`) have their own AGPL / commercial terms on nuget.org / artifex.com.
