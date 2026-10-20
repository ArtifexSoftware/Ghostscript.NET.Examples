using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;

namespace Ghostscript.NET.Examples.InkCoverage;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 15-InkCoverage");
        InkCoverage();
    }

    static void InkCoverage()
    {
        Dictionary<int, GhostscriptPageInkCoverage> pages =
            GhostscriptPdfInfo.GetInkCoverage(ExamplePaths.Input("ink.pdf"));

        string report = ExamplePaths.Output("15-InkCoverage", "ink.txt");
        using var writer = File.CreateText(report);
        foreach (KeyValuePair<int, GhostscriptPageInkCoverage> kv in pages)
        {
            GhostscriptPageInkCoverage c = kv.Value;
            string line = $"Page {kv.Key} C:{c.C:0.##}% M:{c.M:0.##}% Y:{c.Y:0.##}% K:{c.K:0.##}%";
            writer.WriteLine(line);
            ConsoleEx.Info(line);
        }

        ConsoleEx.Done(report);
    }
}
