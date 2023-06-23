using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LibPack.SystemTools;

// ReSharper disable once InconsistentNaming
public sealed record MacOSSharedLibraries(IReadOnlySet<MacOSArchitectureSharedLibraries> ArchitectureSharedLibraries);

// ReSharper disable once InconsistentNaming
public sealed record MacOSArchitectureSharedLibraries(
    string ArchitectureName,
    IReadOnlySet<FilePath> DependencyPaths);

// ReSharper disable once InconsistentNaming
public sealed class MacOSSharedLibrariesTool
{
    public MacOSSharedLibraries Run(FilePath libraryPath)
    {
        var otoolOutput = RunOTool(libraryPath);
        return new MacOSSharedLibraries(
            ParseDependencyLibraryPaths(otoolOutput, libraryPath).ToHashSet());
    }
    
    private IEnumerable<MacOSArchitectureSharedLibraries> ParseDependencyLibraryPaths(
        IEnumerable<string> otoolOutput,
        FilePath libraryPath)
    {
        /*
/path/to/the/lib/libfreetype.6.dylib (architecture x86_64):
	@loader_path/libfreetype.6.dylib (compatibility version 26.0.0, current version 26.0.0)
	/usr/lib/libz.1.dylib (compatibility version 1.0.0, current version 1.2.11)
	/usr/lib/libbz2.1.0.dylib (compatibility version 1.0.0, current version 1.0.8)
	/usr/lib/libSystem.B.dylib (compatibility version 1.0.0, current version 1311.100.3)
/path/to/the/lib/libfreetype.6.dylib (architecture arm64):
	@loader_path/libfreetype.6.dylib (compatibility version 26.0.0, current version 26.0.0)
	/usr/lib/libz.1.dylib (compatibility version 1.0.0, current version 1.2.11)
	/usr/lib/libbz2.1.0.dylib (compatibility version 1.0.0, current version 1.0.8)
	/usr/lib/libSystem.B.dylib (compatibility version 1.0.0, current version 1311.100.3)
         */
        string currentArchitectureName = string.Empty;
        var architectureSharedLibraryPaths = new HashSet<FilePath>();
        foreach (var line in otoolOutput)
        {
            if (TryParseArchitectureLine(line, libraryPath, out var architectureName))
            {
                if (currentArchitectureName != string.Empty)
                {
                    yield return new MacOSArchitectureSharedLibraries(
                        currentArchitectureName,
                        architectureSharedLibraryPaths);
                    architectureSharedLibraryPaths = new();
                }
                
                currentArchitectureName = architectureName;
                continue;
            }

            if (TryParseDependencyPath(line, out var dependencyPath))
            {
                architectureSharedLibraryPaths.Add(dependencyPath);
            }
        }
        
        if (currentArchitectureName != string.Empty)
        {
            yield return new MacOSArchitectureSharedLibraries(
                currentArchitectureName,
                architectureSharedLibraryPaths);
        }
    }

    private bool TryParseArchitectureLine(
        string line, 
        FilePath libraryPath,
        [NotNullWhen(true)] out string? architectureName)
    {
        //`/path/to/the/lib/libfreetype.6.dylib (architecture arm64):`
        var preamble = $"{libraryPath.Value} (architecture ";
        if (line.StartsWith(preamble) &&
            line.EndsWith("):"))
        {
            architectureName = line[preamble.Length..^2];
            return true;
        }
        
        architectureName = default;
        return false;
    }

    private bool TryParseDependencyPath(
        string line,
        out FilePath dependencyPath)
    {
        //`	/usr/lib/libz.1.dylib (compatibility version 1.0.0, current version 1.2.11)`
        var endIndex = line.IndexOf(" (compatibility version", StringComparison.Ordinal);
        if (endIndex < 0)
        {
            dependencyPath = default;
            return false;
        }

        dependencyPath = line.Substring(1, endIndex - 1);
        return true;
    }
    
    private IEnumerable<string> RunOTool(FilePath libraryPath)
    {
        return ProcessRunner.RunGetLines("otool", $"-L {libraryPath.Value}");
    }
}