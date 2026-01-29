using AIKit.Storage.Local;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AIKit.Storage.Tests;

public class LocalVersionedStorageProviderTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly LocalVersionedStorageProvider _provider;
    private readonly ITestOutputHelper _output;

    public LocalVersionedStorageProviderTests(ITestOutputHelper output)
    {
        _output = output;
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
        _output.WriteLine($"Saved file {path} with version {result.Version}");

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
        _output.WriteLine($"Read file {path}, size {result?.Metadata.Size}");

        // Assert
        result.ShouldNotBeNull();
        result!.Metadata.Path.ShouldBe(path);
        result.Metadata.ContentType.ShouldBe("text/plain"); // Detected from .txt extension
        result.Metadata.Size.ShouldBe(content.Length);
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

    [Fact]
    public async Task ReadAsync_WithVersion_ShouldReturnSpecificVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var save1 = await _provider.SaveAsync(path, new MemoryStream(content1));
        var save2 = await _provider.SaveAsync(path, new MemoryStream(content2));

        // Act
        _output.WriteLine($"Reading version {save1.Version}");
        var result = await _provider.ReadAsync(path, save1.Version);

        // Assert
        result.ShouldNotBeNull();
        result!.Metadata.Path.ShouldBe(path);
        result.Metadata.Size.ShouldBe(content1.Length);
        using var reader = new StreamReader(result.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.ShouldBe("Version 1");
    }

    [Fact]
    public async Task DeleteAsync_WithVersion_ShouldRemoveSpecificVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var save1 = await _provider.SaveAsync(path, new MemoryStream(content1));
        var save2 = await _provider.SaveAsync(path, new MemoryStream(content2));

        // Act
        _output.WriteLine($"Deleting version {save1.Version}");
        var deleted = await _provider.DeleteAsync(path, save1.Version);

        // Assert
        deleted.ShouldBeTrue();
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(1);
        versions.First().Version.ShouldBe(save2.Version);
    }

    [Fact]
    public async Task RestoreVersionAsync_WithVersion_ShouldRestoreSpecificVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var save1 = await _provider.SaveAsync(path, new MemoryStream(content1));
        await _provider.SaveAsync(path, new MemoryStream(content2));

        // Act
        _output.WriteLine($"Restoring version {save1.Version}");
        var restoreResult = await _provider.RestoreVersionAsync(path, save1.Version);

        // Assert
        restoreResult.ShouldNotBeNull();
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(3);
        var latest = await _provider.ReadAsync(path);
        using var reader = new StreamReader(latest!.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.ShouldBe("Version 1");
    }

    [Fact]
    public async Task SaveAsync_WithWriteModeCreateNewVersion_ShouldCreateNewVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var options = new StorageWriteOptions { WriteMode = StorageWriteMode.CreateNewVersion };

        // Act
        await _provider.SaveAsync(path, new MemoryStream(content1), options);
        await _provider.SaveAsync(path, new MemoryStream(content2), options);

        // Assert
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(2);
    }

    [Fact]
    public async Task SaveAsync_WithWriteModeReplaceLatest_ShouldReplaceLatestVersion()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var options = new StorageWriteOptions { WriteMode = StorageWriteMode.ReplaceLatest };

        // Act
        await _provider.SaveAsync(path, new MemoryStream(content1));
        await _provider.SaveAsync(path, new MemoryStream(content2), options);

        // Assert
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(1);
        var latest = await _provider.ReadAsync(path);
        using var reader = new StreamReader(latest!.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.ShouldBe("Version 2");
    }

    [Fact]
    public async Task SaveAsync_WithWriteModeFailIfExists_ShouldThrowIfExists()
    {
        // Arrange
        var path = "test/file.txt";
        var content1 = "Version 1"u8.ToArray();
        var content2 = "Version 2"u8.ToArray();
        var options = new StorageWriteOptions { WriteMode = StorageWriteMode.FailIfExists };
        await _provider.SaveAsync(path, new MemoryStream(content1));

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () => await _provider.SaveAsync(path, new MemoryStream(content2), options));
    }

    [Fact]
    public async Task ReadAsync_NonExistingFile_ShouldReturnNull()
    {
        // Act
        var result = await _provider.ReadAsync("nonexistent.txt");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingFile_ShouldReturnFalse()
    {
        // Act
        var deleted = await _provider.DeleteAsync("nonexistent.txt");

        // Assert
        deleted.ShouldBeFalse();
    }

    [Fact]
    public async Task GetMetadataAsync_NonExistingFile_ShouldReturnNull()
    {
        // Act
        var metadata = await _provider.GetMetadataAsync("nonexistent.txt");

        // Assert
        metadata.ShouldBeNull();
    }

    [Fact]
    public async Task ListVersionsAsync_NonExistingFile_ShouldReturnEmpty()
    {
        // Act
        var versions = await _provider.ListVersionsAsync("nonexistent.txt");

        // Assert
        versions.ShouldBeEmpty();
    }

    [Fact]
    public async Task SaveAsync_NullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await _provider.SaveAsync(null!, new MemoryStream("test"u8.ToArray())));
    }

    [Fact]
    public async Task ReadAsync_NullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () => await _provider.ReadAsync(null!));
    }
}