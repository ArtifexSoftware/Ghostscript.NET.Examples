using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Rasterizer;
using SkiaSharp;

namespace Ghostscript.NET.Examples.RasterizeStream;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 02-RasterizeStream");
        RasterizeStream();
    }

    static void RasterizeStream()
    {
        string input = ExamplePaths.Input("sample.pdf");
        string output = ExamplePaths.Output("02-RasterizeStream", "page_001.png");

        using var stream = File.OpenRead(input);
        using var rasterizer = new GhostscriptRasterizer();
        rasterizer.Open(stream, GhostscriptVersionInfo.GetPreferredVersion(), dllFromMemory: false);

        SKBitmap page1 = rasterizer.GetPage(dpi: 150, pageNumber: 1);
        using SKImage sk = SKImage.FromBitmap(page1);
        using SKData data = sk.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream outStream = File.OpenWrite(output);
        data.SaveTo(outStream);

        ConsoleEx.Done(output);
    }
}
