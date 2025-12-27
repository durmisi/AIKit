using AIKit.Core.Ingestion;
using AIKit.Core.Ingestion.Middleware;
using AIKit.Core.Ingestion.Services.Chunking;
using AIKit.Core.Ingestion.Services.ChunkProcessors;
using AIKit.Core.Ingestion.Services.Processors;
using AIKit.Core.Ingestion.Services.Providers;
using AIKit.Core.Ingestion.Services.Writers;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using Xunit;

namespace AIKit.Core.Ingestion.Tests;

public class IngestionPipelineTests
{
    [Fact]
    public async Task Pipeline_ProcessesDocumentsSuccessfully()
    {
        // Arrange
        var testDataDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "TestData"));
        var reader = new MarkdownReader();
        var provider = new FileSystemDocumentProvider(testDataDir, reader, "*.md");

        var processors = new List<IIngestionDocumentProcessor>(); // Skip AI-dependent processors for test

        var chunkingOptions = new ChunkingOptions
        {
            MaxTokensPerChunk = 100,
            OverlapTokens = 10,
            Tokenizer = TiktokenTokenizer.CreateForModel("gpt-4")
        };
        var chunkingStrategy = new SectionBasedChunkingStrategy(chunkingOptions);

        var chunkProcessors = new List<IChunkProcessor>(); // Skip AI-dependent processors

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .Use(next => async ctx => await new ReaderMiddleware(provider, processors).InvokeAsync(ctx, next))
            .Use(next => async ctx => await new DocumentProcessorMiddleware(processors).InvokeAsync(ctx, next))
            .Use(next => async ctx => await new ChunkingMiddleware(chunkingStrategy, chunkProcessors).InvokeAsync(ctx, next))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Documents);
        Assert.True(context.Properties.ContainsKey("chunks"));
        var chunks = (IEnumerable<IngestionChunk<string>>)context.Properties["chunks"];
        Assert.NotEmpty(chunks);
    }
}
