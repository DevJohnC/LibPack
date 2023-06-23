namespace LibPack;

public sealed class LibraryPacker
{
    public NativeLibrary NativeLibrary { get; }

    public LibraryPacker(NativeLibrary nativeLibrary)
    {
        NativeLibrary = nativeLibrary;
    }

    public IReadOnlySet<NativeLibrary> PackTo(string outputPath)
    {
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var packedLibraries = new Dictionary<FilePath, NativeLibrary>();
        PackLibraryTo(outputPath, NativeLibrary, packedLibraries);

        return packedLibraries.Values.ToHashSet();
    }

    private void PackLibraryTo(
        string outputPath,
        NativeLibrary nativeLibrary,
        Dictionary<FilePath,NativeLibrary> packedLibraries)
    {
        if (!CopyToOutputDirectory(nativeLibrary, outputPath, packedLibraries, out var packedLibrary))
            return;

        packedLibraries.Add(packedLibrary.Path, packedLibrary);
        
        //  note: we take dependencies from the packed library because we know it's location for sure and all
        //        tokens like @loader_path have been resolved (@DevJohnC)
        foreach (var dependency in packedLibrary.Dependencies)
        {
            //  skip references to self
            if (dependency.FileName == nativeLibrary.FileName)
                continue;
            
            PackLibraryTo(outputPath, dependency, packedLibraries);
        }
        
        //  note: rewriting the dependency paths on the copy of the library after enumerating dependencies
        //        so that we don't get a list of dependencies we just changed (@DevJohnC)
        RewriteDependencyPaths(packedLibrary);
    }

    private bool CopyToOutputDirectory(
        NativeLibrary nativeLibrary, 
        string outputPath,
        Dictionary<FilePath,NativeLibrary> packedLibraries,
        out NativeLibrary copiedLibrary)
    {
        var copyToFilePath = Path.Combine(outputPath, nativeLibrary.FileName);

        if (packedLibraries.TryGetValue(copyToFilePath, out var existingLibrary))
        {
            copiedLibrary = existingLibrary;
            return false;
        }

        File.Copy(ExpandPath(nativeLibrary), copyToFilePath, true);
        copiedLibrary = NativeLibrary.Require(copyToFilePath);
        return true;
    }

    private string ExpandPath(NativeLibrary nativeLibrary)
    {
        if (nativeLibrary.Path.Value.Contains("@loader_path"))
        {
            return nativeLibrary.Path.Value.Replace("@loader_path", NativeLibrary.DirectoryPath.Value);
        }

        return nativeLibrary.Path.Value;
    }

    private void RewriteDependencyPaths(NativeLibrary nativeLibrary)
    {
        var dependencyRewriter = LibraryDependencyRewriter.RequireForCurrentPlatform();
        
        foreach (var dependencyLibrary in nativeLibrary.Dependencies)
        {
            if (dependencyLibrary.Path.Value.Contains("@loader_path") ||
                dependencyLibrary.FileName == nativeLibrary.FileName)
                continue;

            dependencyRewriter.Replace(
                nativeLibrary.Path,
                dependencyLibrary.Path,
                $"@loader_path/{dependencyLibrary.FileName}");
        }
    }
}