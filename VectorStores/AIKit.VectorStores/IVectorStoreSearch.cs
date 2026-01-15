using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;

namespace AIKit.VectorStores;

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
    IAsyncEnumerable<VectorSearchResult<TRecord>> VectorSearchAsync(
        ReadOnlyMemory<float> vector,
        VectorSearchRequest<TRecord> request);

    /// <summary>
    /// Performs a hybrid search combining vector and keyword search.
    /// </summary>
    /// <param name="vector">The query vector.</param>
    /// <param name="keywords">The keywords for text search.</param>
    /// <param name="request">The hybrid search request parameters.</param>
    /// <returns>An asynchronous enumerable of search results.</returns>
    IAsyncEnumerable<VectorSearchResult<TRecord>> HybridSearchAsync(
        ReadOnlyMemory<float> vector,
        IEnumerable<string> keywords,
        HybridSearchRequest<TRecord> request);
}

/// <summary>
/// Represents a result from a vector search.
/// </summary>
/// <typeparam name="TRecord">The type of the record.</typeparam>
public sealed class VectorSearchResult<TRecord>
{
    /// <summary>
    /// The matching record.
    /// </summary>
    public required TRecord Record { get; init; }

    /// <summary>
    /// The similarity score.
    /// </summary>
    public required double Score { get; init; }

    /// <summary>
    /// Optional metadata associated with the result.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

/// <summary>
/// Request parameters for vector search.
/// </summary>
/// <typeparam name="TRecord">The type of records to search.</typeparam>
public sealed class VectorSearchRequest<TRecord>
{
    /// <summary>
    /// The maximum number of results to return.
    /// </summary>
    public int? Top { get; init; }

    /// <summary>
    /// Whether to include vectors in the results.
    /// </summary>
    public bool IncludeVectors { get; init; }

    /// <summary>
    /// Function to select the vector property from the record.
    /// </summary>
    public Func<TRecord, bool>? VectorProperty { get; init; }

    /// <summary>
    /// Filter function to apply to records.
    /// </summary>
    public Func<TRecord, bool>? Filter { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int Skip { get; internal set; }
}

/// <summary>
/// Request parameters for hybrid search.
/// </summary>
/// <typeparam name="TRecord">The type of records to search.</typeparam>
public sealed class HybridSearchRequest<TRecord>
{
    /// <summary>
    /// The maximum number of results to return.
    /// </summary>
    public int? Top { get; init; }

    /// <summary>
    /// Number of results to skip.
    /// </summary>
    public int Skip { get; internal set; }

    /// <summary>
    /// Function to select the text property from the record.
    /// </summary>
    public Func<TRecord, string>? TextProperty { get; init; }

    /// <summary>
    /// Function to select the vector property from the record.
    /// </summary>
    public Func<TRecord, string>? VectorProperty { get; init; }

    /// <summary>
    /// Filter expression to apply to records.
    /// </summary>
    public Expression<Func<TRecord, bool>>? Filter { get; init; }

    /// <summary>
    /// Whether to include vectors in the results.
    /// </summary>
    public bool IncludeVectors { get; internal set; }
}


internal sealed class SemanticKernelVectorStoreSearch<TRecord>
    : IVectorStoreSearch<TRecord>
{
    private readonly object _collection;

    public SemanticKernelVectorStoreSearch(object collection)
    {
        _collection = collection;
    }

    public IAsyncEnumerable<VectorSearchResult<TRecord>> VectorSearchAsync(
        ReadOnlyMemory<float> vector,
        VectorSearchRequest<TRecord> request)
    {
        if (_collection is not IVectorSearchable<TRecord> searchable)
            throw new NotSupportedException("Vector search not supported.");

        var options = new VectorSearchOptions<TRecord>
        {
            IncludeVectors = request.IncludeVectors,
            Skip = request.Skip,
            VectorProperty = request.VectorProperty != null ? r => request.VectorProperty!(r) : null,
            Filter = request.Filter != null ? r => request.Filter!(r) : null
        };

        return searchable.SearchAsync(vector, request.Top ?? 5, options)
            .Select(r => new VectorSearchResult<TRecord>
            {
                Record = r.Record,
                Score = r.Score ?? 0.0,
                Metadata = null,
            });
    }


    public IAsyncEnumerable<VectorSearchResult<TRecord>> HybridSearchAsync(ReadOnlyMemory<float> vector, IEnumerable<string> keywords, HybridSearchRequest<TRecord> request)
    {
        if (_collection is not IKeywordHybridSearchable<TRecord> hybrid)
            throw new NotSupportedException("Hybrid search not supported.");

        var options = new HybridSearchOptions<TRecord>
        {
            Skip = request.Skip,
            IncludeVectors = request.IncludeVectors,
            Filter = request.Filter,
            VectorProperty = request.VectorProperty != null ? r => request.VectorProperty!(r) : null,
            AdditionalProperty = request.TextProperty != null ? r => request.TextProperty!(r) : null
        };

        return hybrid.HybridSearchAsync(vector, keywords.ToList(), request.Top ?? 5, options)
            .Select(r => new VectorSearchResult<TRecord>
            {
                Record = r.Record,
                Score = r.Score ?? 0.0,
                Metadata = null
            });
    }
}

public sealed class VectorStoreSearchResolver
{
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