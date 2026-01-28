using AIKit.VectorStores.SqlServer;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Shouldly;

namespace AIKit.VectorStores.Tests;

public class SqlServerIntegrationTests : IAsyncLifetime
{
    private IContainer _sqlServerContainer = null!;
    private string _connectionString = null!;
    private bool _dockerAvailable;

    public async Task InitializeAsync()
    {
        try
        {
            _sqlServerContainer = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:latest")
                .WithPortBinding(1433, true)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "StrongPass123!")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "StrongPass123!", "-Q", "SELECT 1"))
                .Build();

            await _sqlServerContainer.StartAsync();

            var port = _sqlServerContainer.GetMappedPublicPort(1433);
            _connectionString = $"Server=localhost,{port};User Id=sa;Password=StrongPass123!;TrustServerCertificate=True;";

            _dockerAvailable = true;
        }
        catch
        {
            _dockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_sqlServerContainer != null)
        {
            await _sqlServerContainer.DisposeAsync();
        }
    }

    [SkippableFact]
    public async Task SqlServerVectorStore_AddDocuments_VerifyAddition()
    {
        Skip.IfNot(_dockerAvailable, "Docker not available for SQL Server");

        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(new SqlServerVectorStoreOptionsConfig
        {
            ConnectionString = _connectionString
        }));
        services.AddSingleton<IEmbeddingGenerator>(new FakeEmbeddingGenerator());
        services.AddSingleton<SqlServerVectorStoreFactory>();

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<SqlServerVectorStoreFactory>();
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