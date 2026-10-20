using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Rasterizer;
using SkiaSharp;

namespace Ghostscript.NET.Examples.RasterizePng;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 01-RasterizePng");
        RasterizePng();
    }

    static void RasterizePng()
    {
        string input = ExamplePaths.Input("sample.pdf");
        var gs = GhostscriptVersionInfo.GetPreferredVersion();

        using var rasterizer = new GhostscriptRasterizer();
        rasterizer.Open(input, gs, dllFromMemory: false);

        string? last = null;
        for (int page = 1; page <= rasterizer.PageCount; page++)
        {
            SKBitmap image = rasterizer.GetPage(dpi: 150, pageNumber: page);
            string output = ExamplePaths.Output("01-RasterizePng", $"page_{page:D3}.png");
            using SKImage sk = SKImage.FromBitmap(image);
            using SKData data = sk.Encode(SKEncodedImageFormat.Png, 100);
            using FileStream stream = File.OpenWrite(output);
            data.SaveTo(stream);
            last = output;
        }

        ConsoleEx.Info($"Pages: {rasterizer.PageCount}");
        ConsoleEx.Done(last);
    }
}
