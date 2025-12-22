using Microsoft.Extensions.VectorData;

namespace AIKit.Core.VectorStores;

public interface IVectorStoreProvider
{
    string Provider { get; }

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// This is the primary method that all providers must support.
    /// </summary>
    VectorStore Create(VectorStoreSettings settings);
}
