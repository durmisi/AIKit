using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;


namespace AIKit.Core.VectorStores;

public interface IVectorStoreFactory
{
    string Provider { get; }

    VectorStore Create();
}



public interface IVectorStoreSearch<TRecord>
{
    IAsyncEnumerable<VectorSearchResult<TRecord>> VectorSearchAsync(
        ReadOnlyMemory<float> vector,
        VectorSearchRequest<TRecord> request);

    IAsyncEnumerable<VectorSearchResult<TRecord>> HybridSearchAsync(
        ReadOnlyMemory<float> vector,
        IEnumerable<string> keywords,
        HybridSearchRequest<TRecord> request);
}


public sealed class VectorSearchRequest<TRecord>
{
    public int Top { get; init; } = 5;
    public int Skip { get; init; }
    public bool IncludeVectors { get; init; }
    public Expression<Func<TRecord, bool>>? Filter { get; init; }
    public Expression<Func<TRecord, object?>>? VectorProperty { get; init; }
}

public sealed class HybridSearchRequest<TRecord>
{
    public int Top { get; init; } = 5;
    public int Skip { get; init; }
    public bool IncludeVectors { get; init; }
    public Expression<Func<TRecord, bool>>? Filter { get; init; }
    public Expression<Func<TRecord, object?>>? VectorProperty { get; init; }
    public Expression<Func<TRecord, object?>>? TextProperty { get; init; }
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
            Skip = request.Skip,
            IncludeVectors = request.IncludeVectors,
            Filter = request.Filter,
            VectorProperty = request.VectorProperty
        };

        return searchable.SearchAsync(vector, request.Top, options);
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
            VectorProperty = request.VectorProperty,
            AdditionalProperty = request.TextProperty
        };

        return hybrid.HybridSearchAsync(vector, keywords.ToList(), request.Top, options);
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