using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class ElasticsearchIntegrationTests : IAsyncLifetime
{
    private IContainer _elasticsearchContainer = null!;
    private Uri _endpoint = null!;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _elasticsearchContainer = new ContainerBuilder()
                .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.11.0")
                .WithPortBinding(9200, true)
                .WithEnvironment("discovery.type", "single-node")
                .WithEnvironment("xpack.security.enabled", "false")
                .WithEnvironment("xpack.security.http.ssl.enabled", "false")
                .WithEnvironment("xpack.license.self_generated.type", "trial")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(9200).ForPath("/_cluster/health?wait_for_status=yellow")))
                .Build();

            await _elasticsearchContainer.StartAsync();

            var port = _elasticsearchContainer.GetMappedPublicPort(9200);
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
        if (_elasticsearchContainer != null)
        {
            await _elasticsearchContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task ElasticsearchVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Elasticsearch");

        // Arrange
        var builder = new AIKit.VectorStores.Elasticsearch.VectorStoreBuilder()
            .WithEndpoint(_endpoint)
            .WithEmbeddingGenerator(new FakeEmbeddingGenerator());
        var store = builder.Build();

        // Define a record type
        var collection = store.GetCollection<string, TestRecord>("test-collection");

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

        // Assert: Verify documents are added
        var retrieved = await collection.GetAsync("doc1");
        retrieved.ShouldNotBeNull();
        retrieved.Key.ShouldBe("doc1");
        retrieved.Text.ShouldBe("This is a document about AI");
    }
}