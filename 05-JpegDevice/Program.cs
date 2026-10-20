using Ghostscript.NET.Examples.Common;
using Ghostscript.NET;

namespace Ghostscript.NET.Examples.JpegDevice;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 05-JpegDevice");
        JpegDevice();
    }

    static void JpegDevice()
    {
        string output = ExamplePaths.Output("05-JpegDevice", "page_%03d.jpg");

        var device = new GhostscriptJpegDevice(GhostscriptJpegDeviceType.Jpeg);
        device.GraphicsAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
        device.TextAlphaBits = GhostscriptImageDeviceAlphaBits.V_4;
        device.ResolutionXY = new GhostscriptImageDeviceResolution(150, 150);
        device.JpegQuality = 85;
        device.InputFiles.Add(ExamplePaths.Input("sample.pdf"));
        device.OutputPath = output;
        device.Process();

        ConsoleEx.Done(output);
    }
}
