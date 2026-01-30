using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class SectionBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;

    public SectionBasedChunkingStrategy(ChunkingOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument dataIngestionDocument)
    {
        if (_options.Tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for section-based chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_options.Tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SectionChunker(chunkerOptions);
        var chunks = await chunker.ProcessAsync(dataIngestionDocument).ToListAsync();

        return chunks;
    }
}