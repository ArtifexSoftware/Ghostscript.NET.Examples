using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;
using Ghostscript.NET.Rasterizer;
using SkiaSharp;

namespace Ghostscript.NET.Examples.AntiAliasCropBox;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 03-AntiAliasCropBox");
        AntiAliasCropBox();
    }

    static void AntiAliasCropBox()
    {
        string input = ExamplePaths.Input("sample.pdf");
        string croppedPdf = ExamplePaths.Output("03-AntiAliasCropBox", "cropped.pdf");
        GhostscriptVersionInfo gs = GhostscriptVersionInfo.GetPreferredVersion();

        // CropBox is PDF points, origin bottom-left: [llx lly urx ury].
        // These values inset sample.pdf (MediaBox ≈ 595×425). Must sit inside the page.
        const string cropBox = "[/CropBox [24 72 500 360] /PAGES pdfmark";

        using (var processor = new GhostscriptProcessor())
        {
            processor.StartProcessing(new[]
            {
                "",
                "-dBATCH", "-dNOPAUSE",
                "-dUseCropBox",
                "-sDEVICE=pdfwrite",
                "-sOutputFile=" + croppedPdf,
                "-c", cropBox,
                "-f", input
            }, stdIO_callback: null);
        }

        ConsoleEx.Info("Wrote CropBox via pdfmark: " + cropBox);
        ConsoleEx.Info("Cropped PDF: " + croppedPdf);

        using var rasterizer = new GhostscriptRasterizer();
        rasterizer.GraphicsAlphaBits = 4;
        rasterizer.TextAlphaBits = 4;
        rasterizer.CustomSwitches.Add("-dUseCropBox");
        rasterizer.Open(croppedPdf, gs, dllFromMemory: false);

        string? last = null;
        for (int page = 1; page <= rasterizer.PageCount; page++)
        {
            SKBitmap image = rasterizer.GetPage(dpi: 300, pageNumber: page);
            string output = ExamplePaths.Output("03-AntiAliasCropBox", $"page_{page:D3}.png");
            using SKImage sk = SKImage.FromBitmap(image);
            using SKData data = sk.Encode(SKEncodedImageFormat.Png, 100);
            using FileStream stream = File.OpenWrite(output);
            data.SaveTo(stream);
            ConsoleEx.Info($"Page {page}: {image.Width} x {image.Height}");
            last = output;
        }

        ConsoleEx.Done(last);
    }
}
