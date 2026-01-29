using AIKit.Storage.Local;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AIKit.Storage.Tests;

/// <summary>
/// Integration tests for storage operations.
/// </summary>
public class StorageIntegrationTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly LocalVersionedStorageProvider _provider;
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageIntegrationTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public StorageIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _provider = new LocalVersionedStorageProvider(_tempDirectory);
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task Save_ReadVersions_Restore_FullIntegration()
    {
        // Arrange
        var path = "documents/report.txt";
        var initialContent = "Initial report content"u8.ToArray();
        var updatedContent = "Updated report content with new data"u8.ToArray();
        var finalContent = "Final report content after review"u8.ToArray();

        // Act & Assert: Step 1 - Save initial version
        var saveResult1 = await _provider.SaveAsync(path, new MemoryStream(initialContent));
        saveResult1.ShouldNotBeNull();
        saveResult1.Path.ShouldBe(path);
        saveResult1.Version.ShouldNotBeNullOrEmpty();
        _output.WriteLine($"Step 1: Saved initial version {saveResult1.Version}");

        // Verify initial save
        var readResult1 = await _provider.ReadAsync(path);
        readResult1.ShouldNotBeNull();
        readResult1!.Metadata.Size.ShouldBe(initialContent.Length);
        using var reader1 = new StreamReader(readResult1.Content);
        var content1 = await reader1.ReadToEndAsync();
        content1.ShouldBe("Initial report content");

        // Act & Assert: Step 2 - Save updated version
        var saveResult2 = await _provider.SaveAsync(path, new MemoryStream(updatedContent));
        saveResult2.ShouldNotBeNull();
        saveResult2.Version.ShouldNotBe(saveResult1.Version); // New version
        _output.WriteLine($"Step 2: Saved updated version {saveResult2.Version}");

        // Verify updated content
        var readResult2 = await _provider.ReadAsync(path);
        readResult2.ShouldNotBeNull();
        readResult2!.Metadata.Size.ShouldBe(updatedContent.Length);
        using var reader2 = new StreamReader(readResult2.Content);
        var content2 = await reader2.ReadToEndAsync();
        content2.ShouldBe("Updated report content with new data");

        // Act & Assert: Step 3 - Read versions
        var versions = await _provider.ListVersionsAsync(path);
        versions.Count.ShouldBe(2);
        versions.ShouldContain(v => v.Version == saveResult1.Version);
        versions.ShouldContain(v => v.Version == saveResult2.Version);
        _output.WriteLine($"Step 3: Listed {versions.Count} versions");

        // Act & Assert: Step 4 - Restore to first version
        var restoreResult = await _provider.RestoreVersionAsync(path, saveResult1.Version);
        restoreResult.ShouldNotBeNull();
        restoreResult.Version.ShouldNotBe(saveResult1.Version); // New version created
        restoreResult.Version.ShouldNotBe(saveResult2.Version);
        _output.WriteLine($"Step 4: Restored to version {restoreResult.Version}");

        // Verify restored content
        var readResult3 = await _provider.ReadAsync(path);
        readResult3.ShouldNotBeNull();
        readResult3!.Metadata.Size.ShouldBe(initialContent.Length);
        using var reader3 = new StreamReader(readResult3.Content);
        var content3 = await reader3.ReadToEndAsync();
        content3.ShouldBe("Initial report content"); // Back to initial

        // Act & Assert: Step 5 - Save final version
        var saveResult3 = await _provider.SaveAsync(path, new MemoryStream(finalContent));
        saveResult3.ShouldNotBeNull();
        _output.WriteLine($"Step 5: Saved final version {saveResult3.Version}");

        // Verify final content
        var readResult4 = await _provider.ReadAsync(path);
        readResult4.ShouldNotBeNull();
        readResult4!.Metadata.Size.ShouldBe(finalContent.Length);
        using var reader4 = new StreamReader(readResult4.Content);
        var content4 = await reader4.ReadToEndAsync();
        content4.ShouldBe("Final report content after review");

        // Final check: Total versions should be 4 (initial, updated, restored, final)
        var finalVersions = await _provider.ListVersionsAsync(path);
        finalVersions.Count.ShouldBe(4);
        _output.WriteLine($"Final: Total {finalVersions.Count} versions");
    }
}