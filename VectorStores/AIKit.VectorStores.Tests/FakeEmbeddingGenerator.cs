using Microsoft.Extensions.AI;

namespace AIKit.VectorStores.Tests;

public class FakeEmbeddingGenerator : IEmbeddingGenerator
{
    public EmbeddingGeneratorMetadata Metadata => new("fake");

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var embeddings = values.Select(v => new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f })).ToList();
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    { }
}