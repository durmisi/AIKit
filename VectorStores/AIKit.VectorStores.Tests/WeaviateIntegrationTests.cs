using AIKit.VectorStores.Weaviate;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Shouldly;
using Xunit;

namespace AIKit.VectorStores.Tests;

[Trait("Category", "RequiresDocker")]
public class WeaviateIntegrationTests : IAsyncLifetime
{
    private IContainer _weaviateContainer = null!;
    private Uri _endpoint = null!;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _weaviateContainer = new ContainerBuilder("semitechnologies/weaviate:latest")
                .WithPortBinding(8080, true)
                .WithEnvironment("AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED", "true")
                .WithEnvironment("PERSISTENCE_DATA_PATH", "/var/lib/weaviate")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(8080).ForPath("/v1/meta")))
                .Build();

            await _weaviateContainer.StartAsync();

            var port = _weaviateContainer.GetMappedPublicPort(8080);
            _endpoint = new Uri($"http://localhost:{port}");

            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_weaviateContainer != null)
        {
            await _weaviateContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task WeaviateVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Weaviate");

        // Arrange
        var builder = new AIKit.VectorStores.Weaviate.VectorStoreBuilder()
            .WithEmbeddingGenerator(new FakeEmbeddingGenerator())
             .WithHttpClient(new HttpClient
             {
                 BaseAddress = new Uri($"{_endpoint}/v1/")
             });
        
        var store = builder.Build();

        // Define a record type
        var collection = store.GetCollection<Guid, TestRecord>("TestCollection");

        await collection.EnsureCollectionExistsAsync();

        // Create test records with vectors
        var records = new[]
        {
            new TestRecord
            {
                Key = Guid.NewGuid(),
                Text = "This is a document about AI",
                Vector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f })
            }
        };

        // Act: Add documents
        foreach (var record in records)
        {
            await collection.UpsertAsync(record);
        }

        // Assert: Verify documents are added
        var retrieved = await collection.GetAsync(records.First().Key);
        retrieved.ShouldNotBeNull();
        retrieved.Text.ShouldBe("This is a document about AI");
    }

    public class TestRecord
    {
        [VectorStoreKey]
        public Guid Key { get; init; }

        [VectorStoreData]
        public string Text { get; init; } = default!;

        [VectorStoreVector(Dimensions: 3)]
        public ReadOnlyMemory<float> Vector { get; init; }
    }
}