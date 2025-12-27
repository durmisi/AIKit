using AIKit.Core.Ingestion.Middleware;
using AIKit.Core.Ingestion.Services.Chunking;
using AIKit.Core.Ingestion.Services.Processors;
using AIKit.Core.Ingestion.Services.Providers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Tests;

public class IngestionPipelineTests
{
    [Fact]
    public async Task Pipeline_ProcessesDocumentsSuccessfully()
    {
        var readers = new Dictionary<string, IngestionDocumentReader>
        {
            { ".md", new MarkdownReader() },
            // { ".docx", new WordDocumentReader() },
            // { ".xlsx", new ExcelDocumentReader() },
            // { ".pptx", new PowerPointDocumentReader() },
        };

        // Arrange
        var source = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData"));
        var provider = new FileSystemDocumentProvider(source, readers);

        var processors = new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>(); // Skip AI-dependent processors for test

        //IChatClient chatClient = new MockChatClient();
        //processors.Add(".jpg", new[] { new ImageAlternativeTextProcessor(new EnricherOptions(chatClient)) });

        var chunkingStrategy = new SectionBasedChunkingStrategy(new ChunkingOptions
        {
            MaxTokensPerChunk = 100,
            OverlapTokens = 10,
            Tokenizer = TiktokenTokenizer.CreateForModel("gpt-4")
        });

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .Use(next => async ctx => await new ReaderMiddleware(provider, processors).InvokeAsync(ctx, next))
            .Use(next => async ctx => await new ChunkingMiddleware(chunkingStrategy, new List<IChunkProcessor>()).InvokeAsync(ctx, next))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Documents);
        Assert.True(context.DocumentChunks.Any());
        var chunks = context.DocumentChunks.Values.SelectMany(c => c);
        Assert.NotEmpty(chunks);
    }
}
