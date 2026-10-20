using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.PsToPdf;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 07-PsToPdf");
        PsToPdf();
    }

    static void PsToPdf()
    {
        string input = ExamplePaths.Input("sample.ps");
        string output = ExamplePaths.Output("07-PsToPdf", "from-ps.pdf");

        using var processor = new GhostscriptProcessor();
        processor.Error += (_, e) => Console.Error.WriteLine(e.Message);
        processor.Process(new[]
        {
            "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
            "-sDEVICE=pdfwrite",
            "-sOutputFile=" + output.Replace('\\', '/'),
            "-f",
            input.Replace('\\', '/')
        });

        ConsoleEx.Done(output);
    }
}
