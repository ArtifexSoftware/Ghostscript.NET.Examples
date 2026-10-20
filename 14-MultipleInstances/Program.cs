using System.Runtime.InteropServices;
using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.MultipleInstances;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 14-MultipleInstances");
        MultipleInstances();
    }

    static void MultipleInstances()
    {
        string a = ExamplePaths.Output("14-MultipleInstances", "job-a.pdf");
        string b = ExamplePaths.Output("14-MultipleInstances", "job-b.pdf");
        string input = ExamplePaths.Input("sample.pdf");

        using var done = new CountdownEvent(2);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try { RunJob(input, a); }
            finally { done.Signal(); }
        });
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try { RunJob(input, b); }
            finally { done.Signal(); }
        });
        done.Wait();

        ConsoleEx.Done(a);
        ConsoleEx.Info("Also wrote: " + b);
    }

    static void RunJob(string input, string output)
    {
        // Load GS from memory on Windows so parallel instances do not share one native DLL mapping.
        bool fromMemory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        using var processor = new GhostscriptProcessor(GhostscriptVersionInfo.GetPreferredVersion(), fromMemory);
        processor.Process(new[]
        {
            "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
            "-sDEVICE=pdfwrite",
            "-sOutputFile=" + output,
            "-f",
            input
        });
    }
}
