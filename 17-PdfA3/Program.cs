using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;

namespace Ghostscript.NET.Examples.PdfA3;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 17-PdfA3");
        PdfA3();
    }

    static void PdfA3()
    {
        GhostscriptVersionInfo gs = GhostscriptVersionInfo.GetPreferredVersion();
        string output = ExamplePaths.Output("17-PdfA3", "invoice-pdfa3-zugferd.pdf");

        // ConvertToPDFA3 currently requires an embedded XML invoice (Factur-X / ZUGFeRD).
        var converter = new global::Ghostscript.NET.PDFA3Converter.PDFA3Converter(gs.DllPath);
        converter.SetZUGFeRDProfile("EN 16931");
        converter.SetZUGFeRDVersion("2.3");
        converter.SetEmbeddedXMLFile(ExamplePaths.Input("factur-x.xml"));
        converter.ConvertToPDFA3(ExamplePaths.Input("invoice.pdf"), output);

        ConsoleEx.Done(output);
    }
}
