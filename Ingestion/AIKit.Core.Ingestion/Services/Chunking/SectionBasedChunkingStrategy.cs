// Adapted from Microsoft.Extensions.DataIngestion.Chunkers.SectionChunker

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

    public async Task<IReadOnlyList<DocumentChunk>> Chunk(IngestionDocument document)
    {
        var chunkerOptions = new IngestionChunkerOptions(_options.Tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SectionChunker(chunkerOptions);

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