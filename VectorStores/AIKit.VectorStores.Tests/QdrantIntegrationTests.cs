using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.VectorData;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class QdrantIntegrationTests : IAsyncLifetime
{
    private IContainer _qdrantContainer = null!;
    private int _grpcPort;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _qdrantContainer = new ContainerBuilder()
                .WithImage("qdrant/qdrant:latest")
                .WithPortBinding(6333, true)
                .WithPortBinding(6334, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(6333).ForPath("/healthz")))
                .Build();

            await _qdrantContainer.StartAsync();

            _grpcPort = (int)_qdrantContainer.GetMappedPublicPort(6334);

            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_qdrantContainer != null)
        {
            await _qdrantContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task QdrantVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Qdrant");

        // Arrange
        var builder = new AIKit.VectorStores.Qdrant.VectorStoreBuilder()
            .WithEmbeddingGenerator(new FakeEmbeddingGenerator())
            .WithHost("localhost")
            .WithPort(_grpcPort);

        var store = builder.Build();

        // Define a record type
        var collection = store.GetCollection<Guid, QdrantTestRecord>("test-collection");

        await collection.EnsureCollectionExistsAsync();

        // Create test records with vectors
        var records = new[]
        {
            new QdrantTestRecord
            {
                Key = Guid.NewGuid(),
                Text = "This is a document about AI",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f })
            },
            new QdrantTestRecord
            {
                Key = Guid.NewGuid(),
                Text = "This is about machine learning",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.4f, 0.5f, 0.6f })
            },
            new QdrantTestRecord
            {
                Key = Guid.NewGuid(),
                Text = "Unrelated topic",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.7f, 0.8f, 0.9f })
            }
        };

        // Act: Add documents
        foreach (var record in records)
        {
            await collection.UpsertAsync(record);
        }

        // Assert: Verify documents are added
        var retrieved = await collection.GetAsync(records[0].Key);
        retrieved.ShouldNotBeNull();
        retrieved.Key.ShouldBe(records[0].Key);
        retrieved.Text.ShouldBe("This is a document about AI");
    }
}

public class QdrantTestRecord
{
    [VectorStoreKey]
    public Guid Key { get; init; }

    [VectorStoreData]
    public string Text { get; init; }

    [VectorStoreVector(Dimensions: 3)]
    public ReadOnlyMemory<float> Vector { get; init; }
}