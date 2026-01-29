using Azure.Core;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.Diagnostics;

namespace AIKit.VectorStores.AzureAISearch;

public sealed class VectorStoreBuilder
{
    public string Provider => "azure-ai-search";

    private string? _endpoint;
    private TokenCredential? _credential;
    private IEmbeddingGenerator? _embeddingGenerator;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    public VectorStoreBuilder WithCredential(TokenCredential credential)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        return this;
    }

    public VectorStoreBuilder WithEmbeddingGenerator(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var client = new SearchIndexClient(new Uri(_endpoint!), _credential!);

        var options = CreateOptions(_embeddingGenerator!);

        return new AzureAISearchVectorStore(client, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            throw new InvalidOperationException("Azure AI Search endpoint is not configured.");
        }

        if (_credential == null)
        {
            throw new InvalidOperationException("Azure AI Search credential is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private static AzureAISearchVectorStoreOptions CreateOptions(
       IEmbeddingGenerator embeddingGenerator,
       Action<AzureAISearchVectorStoreOptions>? configure = null)
    {
        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new()
            {
                WriteIndented = Debugger.IsAttached
            }
        };

        configure?.Invoke(options);
        return options;
    }
}
