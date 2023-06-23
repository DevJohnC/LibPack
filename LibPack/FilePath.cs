namespace LibPack;

public readonly struct FilePath
{
    public readonly string Value;

    public FilePath(string value)
    {
        Value = value;
    }

    public static implicit operator FilePath(string path) => new(path);

    public override string ToString()
    {
        return Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is FilePath filePath)
            return Value.Equals(filePath.Value);
        return false;
    }

    public static bool operator ==(FilePath left, FilePath right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FilePath left, FilePath right)
    {
        return !(left == right);
    }
}