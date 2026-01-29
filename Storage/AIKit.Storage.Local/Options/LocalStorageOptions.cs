namespace AIKit.Storage.Local;

/// <summary>
/// Configuration options for local storage provider.
/// </summary>
public sealed class LocalStorageOptions
{
    /// <summary>
    /// The base path for storing files locally.
    /// </summary>
    public string BasePath { get; set; } = default!;
}