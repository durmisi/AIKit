namespace AIKit.VectorStores.Search;

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