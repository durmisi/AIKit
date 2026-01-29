using System.Linq.Expressions;

namespace AIKit.VectorStores.Search;

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