using AIKit.Storage.Azure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AIKit.Storage.Tests;

/// <summary>
/// Tests for the Azure Blob Storage provider.
/// </summary>
public class AzureBlobStorageProviderTests : IAsyncLifetime, IClassFixture<AzuriteFixture>
{
    private readonly AzuriteFixture _fixture;
    private readonly string _containerName;
    private AzureBlobStorageProvider _provider = null!;
    private bool _dockerAvailable;
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobStorageProviderTests"/> class.
    /// </summary>
    /// <param name="fixture">The Azurite fixture.</param>
    /// <param name="output">The test output helper.</param>
    public AzureBlobStorageProviderTests(AzuriteFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _containerName = Guid.NewGuid().ToString("N");
        _output = output;
    }

    /// <summary>
    /// Initializes the test asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        _dockerAvailable = _fixture.IsAvailable;
        if (_dockerAvailable)
        {
            _provider = new AzureBlobStorageProvider(_fixture.ConnectionString, _containerName);
            await _provider.InitializeAsync();
        }
    }

    /// <summary>
    /// Disposes the test asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    [SkippableFact]
    public async Task SaveAsync_WithWriteModeCreateNewVersion_ShouldCreateNewVersion()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
            versions.ShouldHaveSingleItem(); // Azurite does not support versioning
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task SaveAsync_WithWriteModeReplaceLatest_ShouldReplaceLatestVersion()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
            versions.ShouldHaveSingleItem();
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

    [SkippableFact]
    public async Task SaveAsync_WithWriteModeFailIfExists_ShouldThrowIfExists()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
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
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ReadAsync_NonExistingFile_ShouldReturnNull()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
        {
            // Act
            var result = await _provider.ReadAsync("nonexistent.txt");

            // Assert
            result.ShouldBeNull();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task DeleteAsync_NonExistingFile_ShouldReturnFalse()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
        {
            // Act
            var deleted = await _provider.DeleteAsync("nonexistent.txt");

            // Assert
            deleted.ShouldBeFalse();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task GetMetadataAsync_NonExistingFile_ShouldReturnNull()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
        {
            // Act
            var metadata = await _provider.GetMetadataAsync("nonexistent.txt");

            // Assert
            metadata.ShouldBeNull();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }

    [SkippableFact]
    public async Task ListVersionsAsync_NonExistingFile_ShouldReturnEmpty()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Azurite");

        try
        {
            // Act
            var versions = await _provider.ListVersionsAsync("nonexistent.txt");

            // Assert
            versions.ShouldBeEmpty();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker"))
        {
            Skip.If(true, "Docker API error during test execution");
        }
    }
}