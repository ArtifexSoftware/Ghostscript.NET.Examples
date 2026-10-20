using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.PipedOutput;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 13-PipedOutput");
        PipedOutput();
    }

    static void PipedOutput()
    {
        var pipe = new GhostscriptPipedOutput();
        string handle = "%handle%" + int.Parse(pipe.ClientHandle).ToString("X2");
        string output = ExamplePaths.Output("13-PipedOutput", "from-pipe.pdf");

        try
        {
            using var processor = new GhostscriptProcessor();
            processor.StartProcessing(new[]
            {
                "",
                "-dQUIET", "-dSAFER", "-dBATCH", "-dNOPAUSE", "-dNOPROMPT",
                "-sDEVICE=pdfwrite",
                "-o" + handle,
                "-q",
                "-f", ExamplePaths.Input("sample.ps")
            }, stdIO_callback: null);

            File.WriteAllBytes(output, pipe.Data);
        }
        finally
        {
            pipe.Dispose();
        }

        ConsoleEx.Done(output);
    }
}
