using System;
using System.IO;

namespace Ghostscript.NET.Examples.Common
{
    public static class ExamplePaths
    {
        static readonly Lazy<string> RootLazy = new(FindRoot);

        public static string Root => RootLazy.Value;
        public static string InputRoot => Path.Combine(Root, "Input");
        public static string OutputRoot => Path.Combine(Root, "Output");

        public static string Input(string fileName)
        {
            string path = Path.Combine(InputRoot, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Example input not found: " + path, path);
            return path;
        }

        public static string Output(string exampleName, string fileName)
        {
            string dir = Path.Combine(OutputRoot, exampleName);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, fileName);
        }

        static string FindRoot()
        {
            string? dir = AppContext.BaseDirectory;
            for (int i = 0; i < 12 && !string.IsNullOrEmpty(dir); i++)
            {
                if (File.Exists(Path.Combine(dir, "Ghostscript.NET.Examples.sln")))
                    return dir;
                dir = Directory.GetParent(dir)?.FullName;
            }

            throw new DirectoryNotFoundException(
                "Could not locate Ghostscript.NET.Examples.sln. Run from this solution.");
        }
    }
}
