using Microsoft.Extensions.AI;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class ChunkingOptions
{
    public int MaxTokensPerChunk { get; init; } = 2000;

    public int OverlapTokens { get; init; } = 0;

    public required Tokenizer Tokenizer { get; init; }

    public IEmbeddingGenerator<string, Embedding<float>>? EmbeddingGenerator { get; init; }
}