using AIKit.VectorStores.PgVector;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class PgVectorIntegrationTests : IAsyncLifetime
{
    private IContainer _postgresContainer = null!;
    private string _connectionString = null!;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _postgresContainer = new ContainerBuilder()
                .WithImage("pgvector/pgvector:pg16")
                .WithPortBinding(5432, true)
                .WithEnvironment("POSTGRES_DB", "testdb")
                .WithEnvironment("POSTGRES_USER", "testuser")
                .WithEnvironment("POSTGRES_PASSWORD", "testpass")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready", "-U", "testuser", "-d", "testdb"))
                .Build();

            await _postgresContainer.StartAsync();

            // Enable pgvector extension
            var execResult = await _postgresContainer.ExecAsync(["psql", "-U", "testuser", "-d", "testdb", "-c", "CREATE EXTENSION IF NOT EXISTS vector;"]);
            if (execResult.ExitCode != 0)
            {
                throw new Exception($"Failed to create extension: {execResult.Stderr}");
            }

            var port = _postgresContainer.GetMappedPublicPort(5432);
            _connectionString = $"Host=localhost;Port={port};Database=testdb;Username=testuser;Password=testpass;";

            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task PgVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for PostgreSQL with pgvector");

        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new PostgresVectorStoreOptionsConfig
        {
            ConnectionString = _connectionString
        }));
        services.AddSingleton<IEmbeddingGenerator>(new FakeEmbeddingGenerator());
        services.AddSingleton<PostgresVectorStoreFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<PostgresVectorStoreFactory>();
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

public class FakeEmbeddingGenerator : IEmbeddingGenerator
{
    public EmbeddingGeneratorMetadata Metadata => new("fake");

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var embeddings = values.Select(v => new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f })).ToList();
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    { }
}