using System.Text.RegularExpressions;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class HeaderBasedChunkingStrategy : IChunkingStrategy
{
    private readonly ChunkingOptions _options;

    public HeaderBasedChunkingStrategy(ChunkingOptions options)
    {
        _options = options;
    }

    public IReadOnlyList<DocumentChunk> Chunk(IngestionDocument document)
    {
        var chunks = new List<DocumentChunk>();
        var lines = document.Content.Split('\n');
        var currentChunk = new System.Text.StringBuilder();
        var currentMetadata = new Dictionary<string, object>(document.Metadata);

        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"^#{1,6}\s+"))
            {
                // New header, save previous chunk if not empty
                if (currentChunk.Length > 0)
                {
                    chunks.Add(new DocumentChunk
                    {
                        DocumentId = document.Id,
                        Content = currentChunk.ToString().Trim(),
                        Metadata = new Dictionary<string, object>(currentMetadata)
                    });
                    currentChunk.Clear();
                }
                // Update metadata with header
                currentMetadata["header"] = line.Trim();
            }
            currentChunk.AppendLine(line);
        }

        // Add the last chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(new DocumentChunk
            {
                DocumentId = document.Id,
                Content = currentChunk.ToString().Trim(),
                Metadata = new Dictionary<string, object>(currentMetadata)
            });
        }

        return chunks;
    }
}