namespace AIKit.Core.Ingestion.Services.Chunking;

public static class ChunkingStrategyFactory
{
    public static IChunkingStrategy CreateSemanticSimilarity(ChunkingOptions options)
    {
        return new SemanticSimilarityChunkingStrategy(options);
    }

    public static IChunkingStrategy CreateHeaderBased(ChunkingOptions options)
    {
        return new HeaderBasedChunkingStrategy(options);
    }

    public static IChunkingStrategy CreateSectionBased(ChunkingOptions options)
    {
        return new SectionBasedChunkingStrategy(options);
    }
}