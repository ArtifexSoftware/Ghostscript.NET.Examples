using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.CaptureStdio;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 12-CaptureStdio");
        CaptureStdio();
    }

    static void CaptureStdio()
    {
        string output = ExamplePaths.Output("12-CaptureStdio", "out.pdf");

        using var processor = new GhostscriptProcessor();
        processor.Error += (_, e) => Console.Error.WriteLine(e.Message);
        processor.Process(new[]
        {
            "-dBATCH", "-dNOPAUSE",
            "-sDEVICE=pdfwrite",
            "-sOutputFile=" + output,
            ExamplePaths.Input("sample.pdf")
        }, new ConsoleStdIO());

        ConsoleEx.Done(output);
    }

    sealed class ConsoleStdIO : GhostscriptStdIO
    {
        public ConsoleStdIO() : base(true, true, true) { }

        public override void StdIn(out string input, int count) => input = string.Empty;
        public override void StdOut(string output) => Console.Write(output);
        public override void StdError(string error) => Console.Error.Write(error);
    }
}
