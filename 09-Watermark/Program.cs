using Ghostscript.NET.Examples.Common;
using Ghostscript.NET.Processor;

namespace Ghostscript.NET.Examples.Watermark;

internal static class Program
{
    // Real PDF transparency requires -dALLOWPSTRANSPARENCY (otherwise
    // .setfillconstantalpha is not available) and the pdf14 compositor.
    // /ca 0.25 = 25% opaque gray; page content shows through the letters.
    const string WatermarkPs = @"
<< /PageUsesTransparency true /CompatibilityLevel 1.4 >> setpagedevice
0 .pushpdf14devicefilter
/watermarkText { (CONFIDENTIAL) } def
/watermarkFont { /Helvetica-Bold 72 selectfont } def
/watermarkAngle { 45 } def
/pageWidth { currentpagedevice /PageSize get 0 get } def
/pageHeight { currentpagedevice /PageSize get 1 get } def
<< /EndPage {
    2 eq { pop false } {
        gsave
        0.25 .setfillconstantalpha
        watermarkFont
        .75 setgray
        pageWidth .5 mul pageHeight .5 mul translate
        0 0 moveto watermarkText false charpath flattenpath pathbbox
        4 2 roll pop pop 0 0 moveto watermarkAngle rotate
        -.5 mul exch -.5 mul exch rmoveto
        watermarkText show
        grestore true
    } ifelse
} bind >> setpagedevice";

    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 09-Watermark");
        Watermark();
    }

    static void Watermark()
    {
        string output = ExamplePaths.Output("09-Watermark", "watermarked.pdf");
        try
        {
            if (File.Exists(output))
                File.Delete(output);
        }
        catch (IOException)
        {
            output = ExamplePaths.Output("09-Watermark", "watermarked-new.pdf");
        }

        using var processor = new GhostscriptProcessor();
        processor.Error += (_, e) => Console.Error.WriteLine(e.Message);
        processor.StartProcessing(new[]
        {
            "",
            "-dBATCH", "-dNOPAUSE", "-dNOSAFER",
            "-dALLOWPSTRANSPARENCY",
            "-dCompatibilityLevel=1.4",
            "-sDEVICE=pdfwrite",
            "-sOutputFile=" + output.Replace('\\', '/'),
            "-c", WatermarkPs,
            "-f", ExamplePaths.Input("ink.pdf").Replace('\\', '/')
        }, stdIO_callback: null);

        ConsoleEx.Done(output);
    }
}
