using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Search;

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

        return searchable.SearchAsync(vector, request.Top ?? 5, options);
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

        return hybrid.HybridSearchAsync(vector, keywords.ToList(), request.Top ?? 5, options);
    }
}