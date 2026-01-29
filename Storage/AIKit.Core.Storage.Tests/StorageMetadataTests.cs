using AIKit.Storage;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AIKit.Storage.Tests;

/// <summary>
/// Tests for the StorageMetadata class.
/// </summary>
public class StorageMetadataTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageMetadataTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public StorageMetadataTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var path = "test/file.txt";
        var version = "v1";
        var size = 100L;
        var contentType = "text/plain";
        var customMetadata = new Dictionary<string, string> { ["key"] = "value" };
        var createdAt = DateTimeOffset.Now;

        _output.WriteLine($"Creating StorageMetadata with path: {path}, version: {version}, size: {size}");

        // Act
        var metadata = new StorageMetadata
        {
            Path = path,
            Version = version,
            Size = size,
            ContentType = contentType,
            CustomMetadata = customMetadata,
            CreatedAt = createdAt
        };

        // Assert
        metadata.Path.ShouldBe(path);
        metadata.Version.ShouldBe(version);
        metadata.Size.ShouldBe(size);
        metadata.ContentType.ShouldBe(contentType);
        metadata.CustomMetadata.ShouldBe(customMetadata);
        metadata.CreatedAt.ShouldBe(createdAt);

        _output.WriteLine("All assertions passed for StorageMetadata initialization.");
    }
}