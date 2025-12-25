using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class HeaderBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;

    public HeaderBasedChunkingStrategy(ChunkingOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<DocumentChunk>> Chunk(IngestionDocument document)
    {
        if (_options.Tokenizer is null)
        {
            throw new InvalidOperationException("Tokenizer must be provided for header-based chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_options.Tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new HeaderChunker(chunkerOptions);

        var dataIngestionDocument = new Microsoft.Extensions.DataIngestion.IngestionDocument(document.Content);
        var chunks = new List<DocumentChunk>();
        var asyncChunks = await chunker.ProcessAsync(dataIngestionDocument).ToListAsync();

        foreach (var chunk in asyncChunks)
        {
            chunks.Add(new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunk.Content,
                Metadata = new Dictionary<string, object>(document.Metadata)
            });
        }

        return chunks;
    }
}