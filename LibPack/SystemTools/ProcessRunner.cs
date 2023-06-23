using System.Diagnostics;

namespace LibPack.SystemTools;

public static class ProcessRunner
{
    public static int RunGetExitCode(string fileName, string arguments)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        var process = Process.Start(processInfo);
        if (process == null)
            throw new NullReferenceException($"Failed to execute {fileName}.");

        using (process)
        {
            process.WaitForExit();
            return process.ExitCode;
        }
    }
    
    public static string RunGetAll(string fileName, string arguments)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        var process = Process.Start(processInfo);
        if (process == null)
            throw new NullReferenceException($"Failed to execute {fileName}.");

        using (process)
            return process.StandardOutput.ReadToEnd();
    }
    
    public static IEnumerable<string> RunGetLines(string fileName, string arguments)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        var process = Process.Start(processInfo);
        if (process == null)
            throw new NullReferenceException($"Failed to execute {fileName}.");
        
        using (process)
            foreach (var line in ReadProcessLines(process))
            {
                yield return line;
            }
    }
    
    private static IEnumerable<string> ReadProcessLines(Process process)
    {
        while (true)
        {
            var nextLine = process.StandardOutput.ReadLine();
            if (nextLine == null)
                yield break;
            yield return nextLine;
        }
    }
}