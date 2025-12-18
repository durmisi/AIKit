using Microsoft.Extensions.AI;

namespace AIKit.Core.Clients;

public interface IEmbeddingProvider
{
    string Provider { get; }
    IEmbeddingGenerator<string, Embedding<float>> Create();
    IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings);
}