using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class SemanticSimilarityChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;

    public SemanticSimilarityChunkingStrategy(ChunkingOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument document)
    {
        if (_options.EmbeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator must be provided for semantic similarity chunking.");
        }

        if (_options.Tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for semantic similarity chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_options.Tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SemanticSimilarityChunker(_options.EmbeddingGenerator, chunkerOptions);
        var chunks = await chunker.ProcessAsync(document).ToListAsync();

        return chunks;
    }
}