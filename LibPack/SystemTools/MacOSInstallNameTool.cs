namespace LibPack.SystemTools;

// ReSharper disable once InconsistentNaming
public sealed class MacOSInstallNameTool
{
    public static void Replace(
        FilePath libraryPath,
        FilePath originalPath, 
        FilePath replacementPath)
    {
        var exitCode = ProcessRunner.RunGetExitCode("install_name_tool", $"-change \"{originalPath}\" \"{replacementPath}\" \"{libraryPath}\"");
        if (exitCode != 0)
            //  todo: use a more suitable exception type with appropriate data
            throw new Exception("Failed to change shared library name.");
    }
}