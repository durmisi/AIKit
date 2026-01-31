using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;

namespace AIKit.DataIngestion.Services.Chunking;

public sealed class HeaderBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;
    private Tokenizer _tokenizer;

    public HeaderBasedChunkingStrategy(Tokenizer tokenizer, ChunkingOptions? options = null)
    {
        _options = options ?? new ChunkingOptions();
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    public async Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument dataIngestionDocument)
    {
        if (_tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for header-based chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new HeaderChunker(chunkerOptions);
        var chunks = await chunker.ProcessAsync(dataIngestionDocument).ToListAsync();

        return chunks;
    }
}