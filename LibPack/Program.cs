using System.CommandLine;
using LibPack;

var rootCommand = new RootCommand("Native library packaging tool for macOS and Linux.");

var libPathArg = new Argument<string>("Path to the library to package");
rootCommand.AddArgument(libPathArg);

var packagePathOpt = new Option<string?>("--output", "Output path");
rootCommand.AddOption(packagePathOpt);

var packageSystemLibsOpt = new Option<bool?>(
    "--package-syslibs", 
    "Includes packages found in shared system locations. Useful for linux if you wish to avoid possibly distro specific shared libs. Defaults to `false`.");
rootCommand.AddOption(packageSystemLibsOpt);

var bundleBinaryOpt = new Option<string>(
    "--bundle-with", 
    "Path to library built in another architecture to be bundled into a macOS universal binary")
{
    IsHidden = !OperatingSystem.IsMacOS() 
};
rootCommand.AddOption(bundleBinaryOpt);

rootCommand.SetHandler((libPath, packagePath, bundleBinary, packageSystemLibs) =>
{
    try
    {
        var library = NativeLibrary.Require(libPath);
        var packer = new LibraryPacker(library);

        packagePath ??= Path.Combine(Environment.CurrentDirectory, "packed");
        var packedLibraries = packer.PackTo(packagePath);

        Console.WriteLine($"Packed {packedLibraries.Count} libraries to:");
        Console.WriteLine(packagePath);
        foreach (var packagedLibrary in packedLibraries)
        {
            Console.WriteLine($"\t{packagedLibrary.FileName}");
        }
    }
    catch (FileNotFoundException fileNotFoundException)
    {
        Console.WriteLine($"`{fileNotFoundException.FileName}`: {fileNotFoundException.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}, libPathArg, packagePathOpt, bundleBinaryOpt, packageSystemLibsOpt);

await rootCommand.InvokeAsync(args);