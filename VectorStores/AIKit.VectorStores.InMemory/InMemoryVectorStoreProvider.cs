using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AIKit.VectorStores.InMemory;

public sealed class InMemoryVectorStoreFactory : IVectorStoreFactory
{
    private readonly ILogger<InMemoryVectorStoreFactory>? _logger;

    public string Provider => "in-memory";

    public InMemoryVectorStoreFactory(ILogger<InMemoryVectorStoreFactory>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a default in-memory vector store.
    /// </summary>
    public VectorStore Create()
    {
        _logger?.LogInformation("Creating in-memory vector store");
        return new InMemoryVectorStore();
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryVectorStore(
        this IServiceCollection services)
    {
        // Register the factory
        services.AddSingleton<IVectorStoreFactory, InMemoryVectorStoreFactory>();

        return services;
    }
}