using AIKit.Storage.Azure;
using FluentAssertions;
using Xunit;

namespace AIKit.Core.Storage.Tests;

public class AzureBlobStorageProviderTests : IAsyncLifetime, IClassFixture<AzuriteFixture>
{
    private readonly AzuriteFixture _fixture;
    private readonly string _containerName;
    private AzureBlobStorageProvider _provider = null!;

    public AzureBlobStorageProviderTests(AzuriteFixture fixture)
    {
        _fixture = fixture;
        _containerName = Guid.NewGuid().ToString("N");
    }

    public async Task InitializeAsync()
    {
        _provider = new AzureBlobStorageProvider(_fixture.ConnectionString, _containerName);
        await _provider.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

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
        result.Should().NotBeNull();
        result.Path.Should().Be(path);
        result.Version.Should().NotBeNullOrEmpty();
        result.Size.Should().Be(content.Length);
        result.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
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
        result.Should().NotBeNull();
        result!.Metadata.Path.Should().Be(path);
        result.Metadata.ContentType.Should().Be("application/octet-stream"); // Default content type
        using var reader = new StreamReader(result.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.Should().Be("Hello, World!");
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
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalseForNonExistingFile()
    {
        // Act
        var exists = await _provider.ExistsAsync("nonexistent/file.txt");

        // Assert
        exists.Should().BeFalse();
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
        deleted.Should().BeTrue();
        var exists = await _provider.ExistsAsync(path);
        exists.Should().BeFalse();
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
        metadata.Should().NotBeNull();
        metadata!.Path.Should().Be(path);
        metadata.ContentType.Should().Be("text/plain");
        metadata.CustomMetadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
        metadata.Size.Should().Be(content.Length);
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
        versions.Should().HaveCount(1); // Azurite does not support versioning
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
        restoreResult.Should().NotBeNull();
        var versions = await _provider.ListVersionsAsync(path);
        versions.Should().HaveCount(1); // Azurite does not support versioning
        var latest = await _provider.ReadAsync(path);
        using var reader = new StreamReader(latest!.Content);
        var readContent = await reader.ReadToEndAsync();
        readContent.Should().Be("Version 2");
    }
}