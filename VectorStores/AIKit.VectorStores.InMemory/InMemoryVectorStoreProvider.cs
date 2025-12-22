using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AIKit.VectorStores.InMemory;

public sealed class InMemoryVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "in-memory";

    public VectorStore Create()
    {
        return new InMemoryVectorStore();
    }

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new InMemoryVectorStore();
    }
}