using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AIKit.VectorStores.InMemory;

public sealed class VectorStoreBuilder
{
    private ILogger<VectorStoreBuilder>? _logger;

    public string Provider => "in-memory";

    public VectorStoreBuilder WithLogger(ILogger<VectorStoreBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        return this;
    }

    public VectorStore Build()
    {
        _logger?.LogInformation("Creating in-memory vector store");
        return new InMemoryVectorStore();
    }
}
