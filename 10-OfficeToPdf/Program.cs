using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.OfficeToPdf;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 10-OfficeToPdf");
        OfficeToPdf();
    }

    static void OfficeToPdf()
    {
        if (!GhostscriptVersionInfo.TryGetGhostPdlVersion(out GhostscriptVersionInfo pdl))
        {
            ConsoleEx.Info("Skipping: GhostPDL (gpdldll / libgpdl) was not found.");
            return;
        }

        string? key = Environment.GetEnvironmentVariable("GHOSTSCRIPT_NET_OFFICE_KEY");
        if (!string.IsNullOrWhiteSpace(key))
        {
            GhostscriptOffice.Unlock(key);
            ConsoleEx.Info("Office unlock key: set (GHOSTSCRIPT_NET_OFFICE_KEY)");
        }
        else
        {
            ConsoleEx.Info("Office unlock key: not set (restricted mode, first 3 pages)");
        }

        string input = ExamplePaths.Input("sample.docx");
        string output = ExamplePaths.Output("10-OfficeToPdf", "report.pdf");
        GhostscriptOffice.ConvertToPdf(input, output, pdl);
        ConsoleEx.Done(output);

        string pngPattern = ExamplePaths.Output("10-OfficeToPdf", "page-%03d.png");
        using var processor = GhostscriptProcessor.CreateForInput(input);
        processor.Process(new[]
        {
            "-dBATCH", "-dNOPAUSE",
            "-sDEVICE=png16m",
            "-sOutputFile=" + pngPattern,
            input
        });
        ConsoleEx.Done(pngPattern);
    }
}
