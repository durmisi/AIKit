namespace AIKit.DataIngestion.Services.Chunking;

public sealed class ChunkingOptions
{
    public int MaxTokensPerChunk { get; init; } = 2000;

    public int OverlapTokens { get; init; } = 0;

}