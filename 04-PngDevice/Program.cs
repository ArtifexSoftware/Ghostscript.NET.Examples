using Ghostscript.NET.Examples.Common;
using Ghostscript.NET;

namespace Ghostscript.NET.Examples.PngDevice;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 04-PngDevice");
        PngDevice();
    }

    static void PngDevice()
    {
        string output = ExamplePaths.Output("04-PngDevice", "page_%03d.png");

        var device = new GhostscriptPngDevice(GhostscriptPngDeviceType.Png16m);
        device.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
        device.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
        device.ResolutionXY = new GhostscriptImageDeviceResolution(150, 150);
        device.InputFiles.Add(ExamplePaths.Input("sample.pdf"));
        device.Pdf.FirstPage = 1;
        device.Pdf.LastPage = 5;
        device.OutputPath = output;
        device.Process();

        ConsoleEx.Done(output);
    }
}
