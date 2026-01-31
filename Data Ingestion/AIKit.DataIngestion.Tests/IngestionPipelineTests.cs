using AIKit.DataIngestion;
using AIKit.DataIngestion.Middleware;
using AIKit.DataIngestion.Services.Chunking;
using AIKit.DataIngestion.Services.Processors;
using AIKit.DataIngestion.Services.Providers;
using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;

namespace AIKit.Core.Ingestion.Tests;

public class IngestionPipelineTests
{
    [Fact]
    public async Task Pipeline_ProcessesDocumentsSuccessfully()
    {
        // Arrange
        var readers = new Dictionary<string, IngestionDocumentReader>
        {
            { ".md", new MarkdownReader() },
            // { ".docx", new WordDocumentReader() },
            // { ".xlsx", new ExcelDocumentReader() },
            // { ".pptx", new PowerPointDocumentReader() },
        };

        var provider = new FileSystemFileProvider(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")));

        var processors = new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>(); // Skip AI-dependent processors for test

        //IChatClient chatClient = new MockChatClient();
        //processors.Add(".jpg", new[] { new ImageAlternativeTextProcessor(new EnricherOptions(chatClient)) });

        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        var chunkingStrategy = new SectionBasedChunkingStrategy(
            tokenizer,
            new ChunkingOptions
            {
                MaxTokensPerChunk = 100,
                OverlapTokens = 10
            });

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .Use(next => async (ctx, ct) => await new ReaderMiddleware(provider, readers, processors).InvokeAsync(ctx, next, ct))
            .Use(next => async (ctx, ct) => await new ChunkingMiddleware(chunkingStrategy, new List<IChunkProcessor>()).InvokeAsync(ctx, next, ct))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Documents);
        Assert.True(context.DocumentChunks.Any());
        var chunks = context.DocumentChunks.Values.SelectMany(c => c);
        Assert.NotEmpty(chunks);
    }

    [Fact]
    public async Task Pipeline_HandlesReaderFailure()
    {
        // Arrange
        var readers = new Dictionary<string, IngestionDocumentReader>
        {
            { ".md", new FailingReader() }, // Use a reader that throws
        };

        var provider = new FileSystemFileProvider(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")));

        var processors = new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>();

        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        var chunkingStrategy = new SectionBasedChunkingStrategy(
            tokenizer,
            new ChunkingOptions
            {
                MaxTokensPerChunk = 100,
                OverlapTokens = 10
            });

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .Use(next => async (ctx, ct) => await new ReaderMiddleware(provider, readers, processors).InvokeAsync(ctx, next, ct))
            .Use(next => async (ctx, ct) => await new ChunkingMiddleware(chunkingStrategy, new List<IChunkProcessor>()).InvokeAsync(ctx, next, ct))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Errors);
        Assert.Contains("Simulated reader failure", context.Errors);
    }

    [Fact]
    public async Task Pipeline_HandlesChunkingFailure()
    {
        // Arrange
        var readers = new Dictionary<string, IngestionDocumentReader>
        {
            { ".md", new MarkdownReader() },
        };

        var provider = new FileSystemFileProvider(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")));

        var processors = new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>();

        var chunkingStrategy = new FailingChunkingStrategy(); // Use a strategy that throws

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .Use(next => async (ctx, ct) => await new ReaderMiddleware(provider, readers, processors).InvokeAsync(ctx, next, ct))
            .Use(next => async (ctx, ct) => await new ChunkingMiddleware(chunkingStrategy, new List<IChunkProcessor>()).InvokeAsync(ctx, next, ct))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Errors);
        Assert.Contains("Simulated chunking failure", context.Errors);
    }

    [Fact]
    public async Task Pipeline_HandlesProcessorFailure()
    {
        // Arrange
        var readers = new Dictionary<string, IngestionDocumentReader>
        {
            { ".md", new MarkdownReader() },
        };

        var provider = new FileSystemFileProvider(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")));

        var processors = new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>
        {
            { ".md", new[] { new FailingProcessor() } }
        };

        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        var chunkingStrategy = new SectionBasedChunkingStrategy(
            tokenizer,
            new ChunkingOptions
            {
                MaxTokensPerChunk = 100,
                OverlapTokens = 10
            });

        var context = new DataIngestionContext();

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .UseMiddleware<ErrorHandlingMiddleware>()
            .Use(next => async (ctx, ct) => await new ReaderMiddleware(provider, readers, processors).InvokeAsync(ctx, next, ct))
            .Use(next => async (ctx, ct) => await new ChunkingMiddleware(chunkingStrategy, new List<IChunkProcessor>()).InvokeAsync(ctx, next, ct))
            .Build();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.NotEmpty(context.Errors);
        Assert.Contains("Simulated processor failure", context.Errors);
    }
}

// Mock readers and strategies for testing failures
public class FailingReader : IngestionDocumentReader
{
    public override Task<IngestionDocument> ReadAsync(Stream stream, string name, string extension, CancellationToken cancellationToken = default)
    {
        throw new Exception("Simulated reader failure");
    }
}

public class FailingChunkingStrategy : IChunkingStrategy
{
    public Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument document)
    {
        throw new Exception("Simulated chunking failure");
    }
}

public class FailingProcessor : IIngestionDocumentProcessor
{
    public Task<IngestionDocument> ProcessAsync(IngestionDocument document, CancellationToken cancellationToken = default)
    {
        throw new Exception("Simulated processor failure");
    }
}