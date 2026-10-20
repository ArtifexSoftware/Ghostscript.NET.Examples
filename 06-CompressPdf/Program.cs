using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.CompressPdf;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 06-CompressPdf");
        CompressPdf();
    }

    static void CompressPdf()
    {
        string input = ExamplePaths.Input("sample.pdf");
        string output = ExamplePaths.Output("06-CompressPdf", "compressed.pdf");
        long before = new FileInfo(input).Length;

        using var processor = new GhostscriptProcessor();
        processor.Processing += (_, e) =>
            ConsoleEx.Info($"Page {e.CurrentPage} / {e.TotalPages}");

        processor.Process(new[]
        {
            "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
            "-sDEVICE=pdfwrite",
            "-dPDFSETTINGS=/ebook",
            "-sOutputFile=" + output,
            input
        });

        long after = new FileInfo(output).Length;
        ConsoleEx.Info($"Before: {FormatSize(before)}");
        ConsoleEx.Info($"After:  {FormatSize(after)}");
        ConsoleEx.Done(output);
    }

    static string FormatSize(long bytes)
    {
        return $"{bytes:N0} bytes ({bytes / 1024.0:0.##} KB)";
    }
}
