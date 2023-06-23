namespace LibPack;

public sealed record NativeLibrary(FilePath Path)
{
    private readonly FileInfo _fileInfo = new(Path.Value);
    
    public IEnumerable<NativeLibrary> Dependencies => LibraryDependencyEnumerator.RequireForCurrentPlatform(this);

    public string FileName => _fileInfo.Name;

    public FilePath DirectoryPath => _fileInfo.Directory!.FullName;

    public static NativeLibrary Require(FilePath path)
    {
        if (!File.Exists(path.Value))
            throw new FileNotFoundException("Library file not found.", path.Value);

        return new NativeLibrary(path);
    }
}