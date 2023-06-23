using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LibPack.SystemTools;

namespace LibPack;

public abstract class LibraryDependencyEnumerator : IEnumerable<NativeLibrary>
{
    public NativeLibrary Library { get; }

    protected LibraryDependencyEnumerator(NativeLibrary library)
    {
        Library = library;
    }

    protected abstract IEnumerable<NativeLibrary> Execute();

    public IEnumerator<NativeLibrary> GetEnumerator()
    {
        return Execute().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public static LibraryDependencyEnumerator RequireForCurrentPlatform(NativeLibrary nativeLibrary)
    {
        if (OperatingSystem.IsMacOS())
            return new MacOSLibraryDependencyEnumerator(nativeLibrary);
        
        throw new PlatformNotSupportedException("This platform is not supported.");
    }
}

// ReSharper disable once InconsistentNaming
public sealed class MacOSLibraryDependencyEnumerator : LibraryDependencyEnumerator
{
    public MacOSLibraryDependencyEnumerator(NativeLibrary library) : 
        base(library)
    {
    }

    protected override IEnumerable<NativeLibrary> Execute()
    {
        var sharedLibraryDependencies = new MacOSSharedLibrariesTool()
            .Run(Library.Path);

        foreach (var archSharedLibraries in sharedLibraryDependencies.ArchitectureSharedLibraries)
        {
            foreach (var dependencyPath in archSharedLibraries.DependencyPaths)
            {
                if (!IsSystemLibrary(dependencyPath))
                    yield return new NativeLibrary(dependencyPath);
            }
        }
    }

    private bool IsSystemLibrary(FilePath libraryPath)
    {
        return libraryPath.Value.StartsWith("/usr/lib/") ||
               libraryPath.Value.StartsWith("/System/Library/Frameworks/");
    }
}