using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace AIKit.VectorStores.InMemory;

public sealed class InMemoryVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "in-memory";


    public InMemoryVectorStoreFactory()
    {
    }

    /// <summary>
    /// Creates a default in-memory vector store.
    /// </summary>
    public VectorStore Create()
    {
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