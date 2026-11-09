using Ghostscript.NET;
using Ghostscript.NET.Examples.Common;

namespace Ghostscript.NET.Examples.VolumeLicense;

internal static class Program
{
    static void Main()
    {
        ConsoleEx.Title("Ghostscript.NET / 18-VolumeLicense");
        VolumeLicense();
    }

    static void VolumeLicense()
    {
        Environment.SetEnvironmentVariable("GHOSTSCRIPT_NET_OFFICE_ACTIVITY_VERBOSE", "1");
        IgnoreIncompatibleGhostPdlEnv();
        ConsoleEx.Info("Process: " + (Environment.Is64BitProcess ? "64-bit" : "32-bit"));

        if (!GhostscriptVersionInfo.TryGetGhostPdlVersion(out GhostscriptVersionInfo pdl))
        {
            ConsoleEx.Info("Skipping: GhostPDL (gpdldll / libgpdl) was not found.");
            return;
        }

        List<(string Name, string Key)> keys = LoadKeys();
        if (keys.Count == 0)
        {
            ConsoleEx.Info(
                "Skipping: add Input/office.keys (Name=key lines, gitignored) " +
                "or set GHOSTSCRIPT_NET_OFFICE_KEY / GHOSTSCRIPT_NET_OFFICE_KEY_FILE.");
            return;
        }

        string input = ExamplePaths.Input("sample.docx");
        ConsoleEx.Info("Using GhostPDL: " + pdl.DllPath);
        ConsoleEx.Info("Input: " + input);
        ConsoleEx.Info("Keys: " + keys.Count);

        try
        {
            for (int i = 0; i < keys.Count; i++)
            {
                (string name, string key) = keys[i];
                Console.WriteLine();
                Console.WriteLine("=== " + name + " (" + (i + 1) + "/" + keys.Count + ") ===");

                GhostscriptOffice.ClearUnlock();
                GhostscriptOffice.Unlock(key);

                string pdf = ExamplePaths.Output("18-VolumeLicense", SafeFileName(name) + ".pdf");
                if (File.Exists(pdf))
                    File.Delete(pdf);

                GhostscriptOffice.ConvertToPdf(input, pdf, pdl);

                int flags = GhostscriptOffice.LastFlags;
                bool volume = (flags & GhostscriptOffice.KeyFlagVolumeLicense) != 0;
                bool disableSo = (flags & GhostscriptOffice.KeyFlagDisableSo) != 0;
                ConsoleEx.Info("Wrote: " + pdf);
                ConsoleEx.Info("Message: " + GhostscriptOffice.LastMessage);
                ConsoleEx.Info("key_ok: " + GhostscriptOffice.LastKeyOk);
                ConsoleEx.Info("flags: " + flags + " (0x" + flags.ToString("X") + ")");
                ConsoleEx.Info("product: " + GhostscriptOffice.LastProduct);
                ConsoleEx.Info("documents: " + GhostscriptOffice.LastDocumentCount);
                ConsoleEx.Info("Volume license: " + (volume ? "yes" : "no"));
                ConsoleEx.Info("Disable SO: " + (disableSo ? "yes" : "no"));

                if (volume && !disableSo && GhostscriptOffice.LastDocumentCount > 0)
                {
                    ConsoleEx.Info("Flushing activity...");
                    GhostscriptOffice.FlushActivity();
                }
                else
                {
                    ConsoleEx.Info("Activity monitoring was not started.");
                }
            }
        }
        finally
        {
            GhostscriptOffice.ClearUnlock();
        }

        ConsoleEx.Done();
    }

    static List<(string Name, string Key)> LoadKeys()
    {
        var keys = new List<(string Name, string Key)>();
        string? path = FindKeyFile();
        if (path != null)
        {
            ConsoleEx.Info("Key file: " + path);
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    keys.Add(("Key" + (keys.Count + 1), line));
                    continue;
                }

                string name = line.Substring(0, eq).Trim();
                string key = line.Substring(eq + 1).Trim();
                if (name.Length == 0 || key.Length == 0)
                    continue;

                keys.Add((name, key));
            }

            return keys;
        }

        string? envKey = Environment.GetEnvironmentVariable("GHOSTSCRIPT_NET_OFFICE_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
            keys.Add(("GHOSTSCRIPT_NET_OFFICE_KEY", envKey.Trim()));

        return keys;
    }

    static void IgnoreIncompatibleGhostPdlEnv()
    {
        string expected = Environment.Is64BitProcess ? "gpdldll64.dll" : "gpdldll32.dll";
        foreach (string name in new[] { "GHOSTPDL_DLL", "GPDL_DLL" })
        {
            string? path = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                continue;

            string fileName = Path.GetFileName(path);
            if (fileName.Equals(expected, StringComparison.OrdinalIgnoreCase))
                continue;

            ConsoleEx.Info("Ignoring " + name + " (wrong architecture for this process): " + path);
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    static string? FindKeyFile()
    {
        string? env = Environment.GetEnvironmentVariable("GHOSTSCRIPT_NET_OFFICE_KEY_FILE");
        if (!string.IsNullOrWhiteSpace(env) && File.Exists(env))
            return Path.GetFullPath(env);

        string local = Path.Combine(ExamplePaths.InputRoot, "office.keys");
        return File.Exists(local) ? local : null;
    }

    static string SafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
