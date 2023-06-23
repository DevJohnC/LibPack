using LibPack.SystemTools;

namespace LibPack;

public abstract class LibraryDependencyRewriter
{
    public abstract void Replace(
        FilePath libraryPath,
        FilePath originalPath, 
        FilePath replacementPath);

    public static LibraryDependencyRewriter RequireForCurrentPlatform()
    {
        if (OperatingSystem.IsMacOS())
            return new MacOSLibraryDependencyRewriter();
        
        throw new PlatformNotSupportedException("This platform is not supported.");
    }
}

// ReSharper disable once InconsistentNaming
public sealed class MacOSLibraryDependencyRewriter : LibraryDependencyRewriter
{
    public override void Replace(
        FilePath libraryPath,
        FilePath originalPath, 
        FilePath replacementPath)
    {
        MacOSInstallNameTool.Replace(libraryPath, originalPath, replacementPath);
    }
}