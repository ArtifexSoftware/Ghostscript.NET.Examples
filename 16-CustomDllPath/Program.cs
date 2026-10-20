using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Rasterizer;
using SkiaSharp;

namespace Ghostscript.NET.Examples.CustomDllPath;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 16-CustomDllPath");
        CustomDllPath();
    }

    static void CustomDllPath()
    {
        GhostscriptVersionInfo preferred = GhostscriptVersionInfo.GetPreferredVersion();
        var gs = new GhostscriptVersionInfo(preferred.DllPath);
        ConsoleEx.Info("DLL: " + gs.DllPath);

        string output = ExamplePaths.Output("16-CustomDllPath", "page_001.png");
        using var rasterizer = new GhostscriptRasterizer();
        rasterizer.Open(ExamplePaths.Input("sample.pdf"), gs, dllFromMemory: false);

        SKBitmap page = rasterizer.GetPage(dpi: 96, pageNumber: 1);
        using SKImage sk = SKImage.FromBitmap(page);
        using SKData data = sk.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream stream = File.OpenWrite(output);
        data.SaveTo(stream);

        ConsoleEx.Done(output);
    }
}
