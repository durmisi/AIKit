using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class SemanticSimilarityChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly Tokenizer _tokenizer;

    public SemanticSimilarityChunkingStrategy(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        Tokenizer tokenizer,
        ChunkingOptions? options = null)
    {
        _options = options ?? new ChunkingOptions();
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    public async Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument document)
    {
        if (_embeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator must be provided for semantic similarity chunking.");
        }

        if (_tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for semantic similarity chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SemanticSimilarityChunker(_embeddingGenerator, chunkerOptions);
        var chunks = await chunker.ProcessAsync(document).ToListAsync();

        return chunks;
    }
}