using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;

namespace AIKit.Core.VectorStores;

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

public sealed class VectorSearchResult<TRecord>
{
    public required TRecord Record { get; init; }
    public required double Score { get; init; }
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}


public sealed class VectorSearchRequest<TRecord>
{
    public int? Top { get; init; }
    public bool IncludeVectors { get; init; }
    public Func<TRecord, bool>? VectorProperty { get; init; }
    public Func<TRecord, bool>? Filter { get; init; }
    public int Skip { get; internal set; }
}

public sealed class HybridSearchRequest<TRecord>
{
    public int? Top { get; init; }
    public int Skip { get; internal set; }
    public Func<TRecord, string>? TextProperty { get; init; }
    public Func<TRecord, string>? VectorProperty { get; init; }
    public Expression<Func<TRecord, bool>>? Filter { get; init; }
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