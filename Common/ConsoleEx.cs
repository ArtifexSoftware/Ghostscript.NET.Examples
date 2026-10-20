using System;

namespace Ghostscript.NET.Examples.Common
{
    public static class ConsoleEx
    {
        public static void Title(string text)
        {
            Console.WriteLine("=== " + text + " ===");
        }

        public static void Info(string text) => Console.WriteLine(text);

        public static void Done(string? path = null)
        {
            if (!string.IsNullOrEmpty(path))
                Console.WriteLine("Wrote: " + path);
            Console.WriteLine("Done.");
        }
    }
}
