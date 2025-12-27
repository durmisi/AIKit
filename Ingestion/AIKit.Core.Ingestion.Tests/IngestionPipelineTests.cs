using AIKit.Core.Ingestion.Middleware;
using AIKit.Core.Ingestion.Services.Chunking;
using AIKit.Core.Ingestion.Services.ChunkProcessors;
using AIKit.Core.Ingestion.Services.Processors;
using AIKit.Core.Ingestion.Services.Providers;
using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Tests;

public class IngestionPipelineTests
{
    [Fact]
    public async Task Pipeline_ProcessesDocumentsSuccessfully()
    {
        // Arrange
        var testDataDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "TestData"));

        var processors = new List<IIngestionDocumentProcessor>(); // Skip AI-dependent processors for test



        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .Use(next => async ctx => await new ReaderMiddleware(new FileSystemDocumentProvider(testDataDir, new MarkdownReader(), "*.md"), processors).InvokeAsync(ctx, next))
            .Use(next => async ctx => await new DocumentProcessorMiddleware(processors).InvokeAsync(ctx, next))
            .Use(next => async ctx => await new ChunkingMiddleware(new SectionBasedChunkingStrategy(new ChunkingOptions
            {
                MaxTokensPerChunk = 100,
                OverlapTokens = 10,
                Tokenizer = TiktokenTokenizer.CreateForModel("gpt-4")
            }), new List<IChunkProcessor>()
            {

            }).InvokeAsync(ctx, next))
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
