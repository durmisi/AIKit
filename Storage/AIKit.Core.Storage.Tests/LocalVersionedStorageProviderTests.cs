using AIKit.Storage.Local;
using Shouldly;
using Xunit;

namespace AIKit.Storage.Tests;

public class LocalVersionedStorageProviderTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly LocalVersionedStorageProvider _provider;

    public LocalVersionedStorageProviderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _provider = new LocalVersionedStorageProvider(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldStoreFileAndReturnResult()
    {
        // Arrange
        var path = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        var options = new StorageWriteOptions
        {
            ContentType = "text/plain",
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act
        var result = await _provider.SaveAsync(path, new MemoryStream(content), options);

        // Assert
        result.ShouldNotBeNull();
        result.Path.ShouldBe(path);
        result.Version.ShouldNotBeNullOrEmpty();
        result.Size.ShouldBe(content.Length);
        result.Metadata.ShouldNotBeNull();
        result.Metadata.ShouldContainKey("key");
        result.Metadata["key"].ShouldBe("value");
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnStoredFile()
    {
        // Arrange
        var path = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        await _provider.SaveAsync(path, new MemoryStream(content));

        // Act
        var result = await _provider.ReadAsync(path);

        // Assert
        result.ShouldNotBeNull();
        result!.Metadata.Path.ShouldBe(path);
        result.Metadata.ContentType.ShouldBeNull(); // Not set in save
        using var reader = new StreamReader(result.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.ShouldBe("Hello, World!");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueForExistingFile()
    {
        // Arrange
        var path = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        await _provider.SaveAsync(path, new MemoryStream(content));

        // Act
        var exists = await _provider.ExistsAsync(path);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalseForNonExistingFile()
    {
        // Act
        var exists = await _provider.ExistsAsync("nonexistent/file.txt");

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        // Arrange
        var path = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        await _provider.SaveAsync(path, new MemoryStream(content));

        // Act
        var deleted = await _provider.DeleteAsync(path);

        // Assert
        deleted.ShouldBeTrue();
        var exists = await _provider.ExistsAsync(path);
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnMetadata()
    {
        // Arrange
        var path = "test/file.txt";
        var content = "Hello, World!"u8.ToArray();
        var options = new StorageWriteOptions
        {
            ContentType = "text/plain",
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };
        await _provider.SaveAsync(path, new MemoryStream(content), options);

        // Act
        var metadata = await _provider.GetMetadataAsync(path);

        // Assert
        metadata.ShouldNotBeNull();
        metadata!.Path.ShouldBe(path);
        metadata.ContentType.ShouldBe("text/plain");
        metadata.CustomMetadata.ShouldNotBeNull();
        metadata.CustomMetadata.ShouldContainKey("key");
        metadata.CustomMetadata["key"].ShouldBe("value");
        metadata.Size.ShouldBe(content.Length);
    }

    [Fact]
    public async Task ListVersionsAsync_ShouldReturnVersions()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        await _provider.SaveAsync(path, new MemoryStream(content1));
        await _provider.SaveAsync(path, new MemoryStream(content2));

        // Act
        var versions = await _provider.ListVersionsAsync(path);

        // Assert
        versions.Count.ShouldBe(2);
        versions.First().CreatedAt.ShouldBeGreaterThan(versions.Last().CreatedAt);
    }

    [Fact]
    public async Task RestoreVersionAsync_ShouldCreateNewVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        await _provider.SaveAsync(path, new MemoryStream(content1));
        var saveResult2 = await _provider.SaveAsync(path, new MemoryStream(content2));
        var versionToRestore = saveResult2.Version;

        // Act
        var restoreResult = await _provider.RestoreVersionAsync(path, versionToRestore);

        // Assert
        restoreResult.ShouldNotBeNull();
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(3);
        var latest = await _provider.ReadAsync(path);
        using var reader = new StreamReader(latest!.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.ShouldBe("Version 2");
    }
}