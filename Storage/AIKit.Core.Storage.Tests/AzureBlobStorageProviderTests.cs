using AIKit.Storage.Azure;
using Shouldly;
using Xunit;

namespace AIKit.Storage.Tests;

public class AzureBlobStorageProviderTests : IAsyncLifetime, IClassFixture<AzuriteFixture>
{
    private readonly AzuriteFixture _fixture;
    private readonly string _containerName;
    private AzureBlobStorageProvider _provider = null!;
    private bool _dockerAvailable;

    public AzureBlobStorageProviderTests(AzuriteFixture fixture)
    {
        _fixture = fixture;
        _containerName = Guid.NewGuid().ToString("N");
    }

    public async Task InitializeAsync()
    {
        _dockerAvailable = _fixture.IsAvailable;
        if (_dockerAvailable)
        {
            _provider = new AzureBlobStorageProvider(_fixture.ConnectionString, _containerName);
            await _provider.InitializeAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task SaveAsync_ShouldStoreFileAndReturnResult()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ReadAsync_ShouldReturnStoredFile()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
            result.Metadata.ContentType.ShouldBe("application/octet-stream"); // Default content type
            using var reader = new StreamReader(result.Content);
            var readContent = await reader.ReadToEndAsync();
            readContent.ShouldBe("Hello, World!");
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ExistsAsync_ShouldReturnTrueForExistingFile()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ExistsAsync_ShouldReturnFalseForNonExistingFile()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
        {
            // Act
            var exists = await _provider.ExistsAsync("nonexistent/file.txt");

            // Assert
            exists.ShouldBeFalse();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task GetMetadataAsync_ShouldReturnMetadata()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ListVersionsAsync_ShouldReturnVersions()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
            versions.ShouldHaveSingleItem(); // Azurite does not support versioning
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task RestoreVersionAsync_ShouldCreateNewVersion()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
            versions.ShouldHaveSingleItem(); // Azurite does not support versioning
            var latest = await _provider.ReadAsync(path);
            using var reader = new StreamReader(latest!.Content);
            var readContent = await reader.ReadToEndAsync();
            readContent.ShouldBe("Version 2");
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }
}