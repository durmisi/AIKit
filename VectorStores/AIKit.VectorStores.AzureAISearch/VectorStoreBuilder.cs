using Azure.Core;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.Diagnostics;

namespace AIKit.VectorStores.AzureAISearch;

public sealed class VectorStoreBuilder
{
    public string Provider => "azure-ai-search";

    private readonly string _endpoint;
    private readonly TokenCredential _credential;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public VectorStoreBuilder(
        string endpoint,
        TokenCredential credential,
        IEmbeddingGenerator embeddingGenerator)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    public VectorStore Build()
    {
        Validate();

        var client = new SearchIndexClient(new Uri(_endpoint), _credential);

        var options = CreateOptions(_embeddingGenerator);

        return new AzureAISearchVectorStore(client, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_endpoint?.ToString()))
        {
            throw new InvalidOperationException("Azure AI Search endpoint is not configured.");
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
