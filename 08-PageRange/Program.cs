using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.PageRange;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 08-PageRange");
        PageRange();
    }

    static void PageRange()
    {
        const int firstPage = 2;
        const int lastPage = 4;
        string input = ExamplePaths.Input("ink.pdf");
        string outputPattern = ExamplePaths.Output("08-PageRange", "page-%03d.png");
        string outputDir = Path.GetDirectoryName(outputPattern)!;

        foreach (string stale in Directory.GetFiles(outputDir, "page-*.png"))
            File.Delete(stale);

        ConsoleEx.Info($"Input: {Path.GetFileName(input)}");
        ConsoleEx.Info($"Range: PDF pages {firstPage}-{lastPage}");

        using var processor = new GhostscriptProcessor();
        processor.Processing += (_, e) =>
            ConsoleEx.Info($"Rendering {e.CurrentPage} / {e.TotalPages}");

        processor.Process(new[]
        {
            "-dSAFER", "-dBATCH", "-dNOPAUSE",
            "-dFirstPage=" + firstPage,
            "-dLastPage=" + lastPage,
            "-sDEVICE=png16m",
            "-r96",
            "-dTextAlphaBits=4",
            "-dGraphicsAlphaBits=4",
            "-sOutputFile=" + outputPattern.Replace('\\', '/'),
            "-f",
            input.Replace('\\', '/')
        });

        string[] files = Directory.GetFiles(outputDir, "page-*.png");
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < files.Length; i++)
        {
            int pdfPage = firstPage + i;
            ConsoleEx.Info($"{Path.GetFileName(files[i])}  <- PDF page {pdfPage}");
        }

        ConsoleEx.Info($"Wrote {files.Length} of {lastPage - firstPage + 1} pages in the range.");
        ConsoleEx.Done(files.Length > 0 ? files[^1] : outputDir);
    }
}
