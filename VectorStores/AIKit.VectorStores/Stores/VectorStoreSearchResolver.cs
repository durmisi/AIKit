using AIKit.VectorStores.Search;
using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Stores;

public sealed class VectorStoreSearchResolver
{
    /// <summary>
    /// Resolves a vector store search instance for the specified collection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TRecord">The type of the record.</typeparam>
    /// <param name="vectorStore">The vector store instance.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>An <see cref="IVectorStoreSearch{TRecord}"/> instance.</returns>
    public IVectorStoreSearch<TRecord> Resolve<TKey, TRecord>(
        VectorStore vectorStore,
        string collectionName)
        where TKey : notnull
        where TRecord : class
    {
        var collection = vectorStore.GetCollection<TKey, TRecord>(collectionName, null);
        return new SemanticKernelVectorStoreSearch<TRecord>(collection);
    }
}