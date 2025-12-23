using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AIKit.VectorStores.InMemory;

public sealed class InMemoryVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "in-memory";

    /// <summary>
    /// Creates a default in-memory vector store.
    /// </summary>
    public VectorStore Create()
    {
        return new InMemoryVectorStore();
    }

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new InMemoryVectorStore();
    }
}