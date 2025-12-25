namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class TokenBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;

    public TokenBasedChunkingStrategy(ChunkingOptions options)
    {
        _options = options;
    }

    public async Task<IReadOnlyList<DocumentChunk>> Chunk(IngestionDocument document)
    {
        var chunks = new List<DocumentChunk>();
        var tokens = _options.Tokenizer.EncodeToIds(document.Content);
        var start = 0;

        while (start < tokens.Count)
        {
            var end = Math.Min(start + _options.MaxTokensPerChunk, tokens.Count);
            var chunkTokens = tokens.Skip(start).Take(end - start).ToList();
            var chunkContent = _options.Tokenizer.Decode(chunkTokens);

            chunks.Add(new DocumentChunk
            {
                DocumentId = document.Id,
                Content = chunkContent,
                Metadata = new Dictionary<string, object>(document.Metadata)
                {
                    ["startToken"] = start,
                    ["endToken"] = end
                }
            });

            start = end - _options.OverlapTokens;
            if (start >= tokens.Count) break;
        }

        return chunks;
    }
}