using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.Print;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 11-Print");
        Print();
    }

    static void Print()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("GHOSTSCRIPT_NET_RUN_PRINTER"), "1",
                StringComparison.OrdinalIgnoreCase))
        {
            ConsoleEx.Info("Skipping: set GHOSTSCRIPT_NET_RUN_PRINTER=1 to send to the printer.");
            return;
        }

        using var processor = new GhostscriptProcessor();
        processor.StartProcessing(new[]
        {
            "",
            "-dPrinted", "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
            "-dNumCopies=1",
            "-sDEVICE=mswinpr2",
            "-sOutputFile=%printer%Microsoft Print to PDF",
            "-f",
            ExamplePaths.Input("sample.pdf")
        }, stdIO_callback: null);

        ConsoleEx.Done();
    }
}
