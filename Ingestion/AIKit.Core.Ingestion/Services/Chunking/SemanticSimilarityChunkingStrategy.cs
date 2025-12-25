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

    public IReadOnlyList<DocumentChunk> Chunk(IngestionDocument document)
    {
        if (_options.EmbeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator must be provided for semantic similarity chunking.");
        }

        var chunkerOptions = new IngestionChunkerOptions(_options.Tokenizer)
        {
            MaxTokensPerChunk = _options.MaxTokensPerChunk,
            OverlapTokens = _options.OverlapTokens
        };

        var chunker = new SemanticSimilarityChunker(_options.EmbeddingGenerator, chunkerOptions);

        var dataIngestionDocument = new Microsoft.Extensions.DataIngestion.IngestionDocument(document.Content);
        var chunks = new List<DocumentChunk>();
        var asyncChunksTask = Task.Run(async () =>
        {
            var list = new List<Microsoft.Extensions.DataIngestion.IngestionChunk<string>>();
            await foreach (var chunk in chunker.ProcessAsync(dataIngestionDocument))
            {
                list.Add(chunk);
            }
            return list;
        });
        var asyncChunks = asyncChunksTask.GetAwaiter().GetResult();
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