using AIKit.VectorStores.InMemory;
using Microsoft.Extensions.VectorData;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class VectorStoreIntegrationTests
{
    [Fact]
    public async Task InMemoryVectorStore_AddDocuments_VerifyAddition()
    {
        // Arrange
        var factory = new InMemoryVectorStoreFactory();
        var store = factory.Create();

        // Define a record type
        var collection = store.GetCollection<string, TestRecord>("test-collection");

        // 🔑 Create collection explicitly
        await collection.EnsureCollectionExistsAsync();

        // Create test records with vectors
        var records = new[]
        {
            new TestRecord
            {
                Key = "doc1",
                Text = "This is a document about AI",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f })
            },
            new TestRecord
            {
                Key = "doc2",
                Text = "This is about machine learning",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.4f, 0.5f, 0.6f })
            },
            new TestRecord
            {
                Key = "doc3",
                Text = "Unrelated topic",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.7f, 0.8f, 0.9f })
            }
        };

        // Act: Add documents
        foreach (var record in records)
        {
            await collection.UpsertAsync(record);
        }

        // Verify documents are added by trying to get one
        var retrieved = await collection.GetAsync("doc1");
        retrieved.ShouldNotBeNull();
        retrieved.Key.ShouldBe("doc1");
        retrieved.Text.ShouldBe("This is a document about AI");
    }
}

public class TestRecord
{
    [VectorStoreKey]
    public string Key { get; init; }

    [VectorStoreData]
    public string Text { get; init; }

    [VectorStoreVector(Dimensions: 3)]
    public ReadOnlyMemory<float> Vector { get; init; }
}