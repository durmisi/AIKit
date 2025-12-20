using Microsoft.Extensions.AI;

namespace AIKit.Core.Vector;

public sealed class VectorStoreSettings
{
    public string? Endpoint { get; set; }
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? ApiKey { get; set; }
    public IEmbeddingGenerator? EmbeddingGenerator { get; internal set; }
}
