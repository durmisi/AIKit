using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Stores;

/// <summary>
/// Factory for creating vector store instances.
/// </summary>
public interface IVectorStoreFactory
{
    /// <summary>
    /// Gets the name of the vector store provider (e.g., "in-memory", "qdrant").
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Creates a new instance of the vector store.
    /// </summary>
    /// <returns>A <see cref="VectorStore"/> instance.</returns>
    VectorStore Create();
}