using AIKit.VectorStores.Redis;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class RedisIntegrationTests : IAsyncLifetime
{
    private IContainer _redisContainer = null!;
    private string _endpoint = null!;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _redisContainer = new ContainerBuilder()
                .WithImage("redis/redis-stack:latest")
                .WithPortBinding(6379, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("redis-cli", "ping"))
                .Build();

            await _redisContainer.StartAsync();

            var port = _redisContainer.GetMappedPublicPort(6379);
            _endpoint = $"localhost:{port}";

            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task RedisVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for Redis");

        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new RedisVectorStoreOptionsConfig
        {
            Endpoint = _endpoint
        }));
        services.AddSingleton<IEmbeddingGenerator>(new FakeEmbeddingGenerator());
        services.AddSingleton<RedisVectorStoreFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<RedisVectorStoreFactory>();
        var store = factory.Create();

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