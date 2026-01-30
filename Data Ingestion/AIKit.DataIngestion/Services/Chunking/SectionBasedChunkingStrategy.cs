using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class SectionBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;
    private Tokenizer _tokenizer;

    public SectionBasedChunkingStrategy(Tokenizer tokenizer, ChunkingOptions? options = null )
    {
        _options = options ?? new ChunkingOptions();
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    public async Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument dataIngestionDocument)
    {
        if (_tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for section-based chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SectionChunker(chunkerOptions);
        var chunks = await chunker.ProcessAsync(dataIngestionDocument).ToListAsync();

        return chunks;
    }
}