namespace AIKit.Storage;

public sealed class StorageWriteResult
{
    public string Path { get; }
    public string Version { get; }
    public long Size { get; }
    public IDictionary<string, string>? Metadata { get; }

    public StorageWriteResult(
        string path,
        string version,
        long size,
        IDictionary<string, string>? metadata = null)
    {
        Path = path;
        Version = version;
        Size = size;
        Metadata = metadata;
    }
}