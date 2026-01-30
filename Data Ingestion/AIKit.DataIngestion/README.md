# AIKit.Core.Ingestion

This library provides a middleware pipeline pattern for building extensible ingestion workflows in AIKit, adapted from the DataIngestion project.

## Overview

The `IngestionPipeline<T>` allows chaining middleware components that process a context of type `T` asynchronously. Middleware can perform operations like document reading, processing, chunking, and writing. The library supports logging throughout the pipeline and integrates with AIKit's vector stores for RAG workflows.

## Key Components

- `IngestionPipeline<T>`: Executes a chain of middleware on a context, with logging support.
- `IngestionPipelineBuilder<T>`: Fluent builder for constructing pipelines with `Use`, `UseMiddleware`, and `WithLoggerFactory` methods.
- `IIngestionMiddleware<TContext>`: Interface for middleware implementations.
- `IngestionDelegate<TContext>`: Delegate representing the next step in the pipeline.
- `IngestionContext`: Base context class with an `Errors` list for tracking issues.
- `DataIngestionContext`: Specialized context for data ingestion with `Documents`, `Properties`, and `LoggerFactory`.
- `IIngestionDocumentProvider`: Interface for document readers (e.g., file system, Markdown).
- `IChunkingStrategy`: Interface for document chunking strategies (token-based, semantic, header-based).
- `IDocumentWriter`: Interface for writing chunks to vector stores.
- `IChunkProcessor`: Interface for chunk-level processing.

## Building a Pipeline

To build a pipeline:

1. Define your context class, inheriting from `IngestionContext` or use `DataIngestionContext`.
2. Implement or use existing middleware, document providers, chunking strategies, and writers.
3. Use `IngestionPipelineBuilder<T>` for fluent construction.
4. Configure logging with `WithLoggerFactory`.
5. Call `ExecuteAsync(context)` to run the pipeline.

## Example

```csharp
using AIKit.Core.Ingestion;
using AIKit.Core.Ingestion.Middleware;
using AIKit.Core.Ingestion.Services.Readers;
using AIKit.Core.Ingestion.Services.Chunking;
using AIKit.Core.Ingestion.Services.Writers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;

// Create logger factory
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole());

// Configure embedding generator (requires AIKit clients)
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    // e.g., openAIClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// Define context
var context = new DataIngestionContext();

// Configure chunking options
var chunkingOptions = new ChunkingOptions
{
    Tokenizer = TiktokenTokenizer.CreateForModel("gpt-4"),
    MaxTokensPerChunk = 2000,
    OverlapTokens = 0,
    EmbeddingGenerator = embeddingGenerator  // For semantic chunking
};

// Configure document processor
EnricherOptions enricherOptions = new(chatClient)
{
    // Enricher failures should not fail the whole ingestion pipeline, as they are best-effort enhancements.
    // This logger factory can be used to create loggers to log such failures.
    LoggerFactory = loggerFactory
};

IngestionDocumentProcessor imageAlternativeTextEnricher = new ImageAlternativeTextEnricher(enricherOptions);
IngestionChunkProcessor<string> summaryEnricher = new SummaryEnricher(enricherOptions);

var processors = new IDocumentProcessor[]
{
    new MarkdownProcessor(),  // Parse Markdown to HTML
    new ImageAlternativeTextProcessor(imageAlternativeTextEnricher),
};

var chunkProcessors = new IChunkProcessor[]
{
    new SummaryChunkProcessor(summaryEnricher)
};

// Build pipeline
var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
    .UseMiddleware<ErrorHandlingMiddleware<DataIngestionContext>>()
    .Use(next => new ReaderMiddleware(new FileSystemDocumentProvider(new DirectoryInfo("./data"), "*.md"), processors).InvokeAsync(ctx, next))  // With processors
    .UseMiddleware<ChunkingMiddleware>(ChunkingStrategyFactory.CreateSemanticSimilarity(chunkingOptions), chunkProcessors)
    .UseMiddleware<WriterMiddleware>(/* writer */)
    .WithLoggerFactory(loggerFactory)
    .Build();

// Execute
await pipeline.ExecuteAsync(context);

// Check results
if (context.Errors.Any())
{
    // Handle errors
}
else
{
    // Ingestion complete
}
```

## Middleware Execution Order

Middleware is executed in the order added, but chained in reverse (last added runs first).

## Error Handling

Use `ErrorHandlingMiddleware<T>` to catch exceptions and log errors.

## Document Providers

Implement `IIngestionDocumentProvider` for custom readers:

- `FileSystemDocumentProvider`: Reads files from a directory with search patterns, supports Markdown parsing.

## Document Processors

Implement `IDocumentProcessor` for custom document processing:

- `ImageAlternativeTextProcessor`: Enriches images with AI-generated alternative text (adapts `ImageAlternativeTextEnricher`).
- `MarkdownProcessor`: Parses Markdown content to HTML and adds to metadata.
- Add custom processors for AI enrichments like summarization or metadata extraction.

Configure with `EnricherOptions` including chat client and logger factory.

## Chunking Strategies

Implement `IChunkingStrategy` for custom chunking:

- `TokenBasedChunkingStrategy`: Splits by token count.
- `SemanticSimilarityChunkingStrategy`: Splits by semantic similarity (requires embeddings). Falls back to token-based if embeddings unavailable.
- `HeaderBasedChunkingStrategy`: Splits by Markdown headers.
- `SectionBasedChunkingStrategy`: Splits by sections.

Configure with `ChunkingOptions` including tokenizer, embedding generator, and limits.

## Chunk Processors

Implement `IChunkProcessor` for custom chunk processing:

- `SummaryChunkProcessor`: Generates AI summaries for chunks (adapts `SummaryEnricher`).

Configure with `EnricherOptions` including chat client and logger factory.

## Logging

All components support logging via `ILoggerFactory`:

- Pipeline logs execution start/completion.
- Middlewares log operations (e.g., document count, chunk count).
- Provide `ILoggerFactory` to the builder for console or other logging.

## Integration with Vector Stores

Use `IDocumentWriter` implementations to write chunks to AIKit vector stores like `AIKit.VectorStores.SqliteVec` for RAG.

## Extensibility

- Custom middleware via `IIngestionMiddleware<T>`.
- Custom providers via `IIngestionDocumentProvider`.
- Custom strategies via `IChunkingStrategy`.
- Custom writers via `IDocumentWriter`.
- Integrate AI enrichments using AIKit clients- Integrate AI enrichments using AIKit clients