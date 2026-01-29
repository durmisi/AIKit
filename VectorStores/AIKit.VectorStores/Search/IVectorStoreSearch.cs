namespace AIKit.VectorStores.Search;

/// <summary>
/// Provides search capabilities for vector stores.
/// </summary>
/// <typeparam name="TRecord">The type of records stored in the vector store.</typeparam>
public interface IVectorStoreSearch<TRecord>
{
    /// <summary>
    /// Performs a vector similarity search.
    /// </summary>
    /// <param name="vector">The query vector.</param>
    /// <param name="request">The search request parameters.</param>
    /// <returns>An asynchronous enumerable of search results.</returns>
    IAsyncEnumerable<Microsoft.Extensions.VectorData.VectorSearchResult<TRecord>> VectorSearchAsync(
        ReadOnlyMemory<float> vector,
        VectorSearchRequest<TRecord> request);

    /// <summary>
    /// Performs a hybrid search combining vector and keyword search.
    /// </summary>
    /// <param name="vector">The query vector.</param>
    /// <param name="keywords">The keywords for text search.</param>
    /// <param name="request">The hybrid search request parameters.</param>
    /// <returns>An asynchronous enumerable of search results.</returns>
    IAsyncEnumerable<Microsoft.Extensions.VectorData.VectorSearchResult<TRecord>> HybridSearchAsync(
        ReadOnlyMemory<float> vector,
        IEnumerable<string> keywords,
        HybridSearchRequest<TRecord> request);
}