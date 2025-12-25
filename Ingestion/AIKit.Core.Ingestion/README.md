# AIKit.Core.Ingestion

This library provides a middleware pipeline pattern for building extensible ingestion workflows in AIKit.

## Overview

The `IngestionPipeline<T>` allows chaining middleware components that process a context of type `T` asynchronously. Middleware can perform operations like data validation, transformation, or error handling.

## Key Components

- `IngestionPipeline<T>`: Executes a chain of middleware on a context.
- `IngestionPipelineBuilder<T>`: Fluent builder for constructing pipelines with `Use` methods.
- `IIngestionMiddleware<TContext>`: Interface for middleware implementations.
- `IngestionDelegate<TContext>`: Delegate representing the next step in the pipeline.
- `IngestionContext`: Base context class with an `Errors` list for tracking issues.

## Building a Pipeline

To build a pipeline:

1. Define your context class, inheriting from `IngestionContext` if error tracking is needed.
2. Implement middleware by creating classes that implement `IIngestionMiddleware<TContext>`.
3. Create a list of middleware factories (functions that take the next delegate and return a new delegate).
4. Instantiate `IngestionPipeline<T>` with the list of factories.
5. Call `ExecuteAsync(context)` to run the pipeline.

Alternatively, use the `IngestionPipelineBuilder<T>` for a fluent API:

1. Create an `IngestionPipelineBuilder<T>` instance.
2. Chain `Use` or `UseMiddleware` calls to add middleware.
3. Call `Build()` to create the pipeline.

## Example

```csharp
using AIKit.Core.Ingestion;
using AIKit.Core.Ingestion.Middleware;

// Define a custom context
public class MyIngestionContext : IngestionContext
{
    public string Data { get; set; }
}

// Implement middleware
public class ValidationMiddleware : IIngestionMiddleware<MyIngestionContext>
{
    public async Task InvokeAsync(MyIngestionContext ctx, IngestionDelegate<MyIngestionContext> next)
    {
        if (string.IsNullOrEmpty(ctx.Data))
        {
            ctx.Errors.Add("Data is required");
            return;
        }
        await next(ctx);
    }
}

public class ProcessingMiddleware : IIngestionMiddleware<MyIngestionContext>
{
    public async Task InvokeAsync(MyIngestionContext ctx, IngestionDelegate<MyIngestionContext> next)
    {
        // Process data
        ctx.Data = ctx.Data.ToUpper();
        await next(ctx);
    }
}

// Build the pipeline using the builder
var pipeline = new IngestionPipelineBuilder<MyIngestionContext>()
    .Use(next => async ctx => await new ValidationMiddleware().InvokeAsync(ctx, next))
    .Use(next => async ctx => await new ProcessingMiddleware().InvokeAsync(ctx, next))
    .UseMiddleware<ErrorHandlingMiddleware<MyIngestionContext>>()
    .Build();

// Execute
var context = new MyIngestionContext { Data = "hello" };
await pipeline.ExecuteAsync(context);

// Check results
if (context.Errors.Any())
{
    // Handle errors
}
else
{
    // Use processed data
}
```

## Middleware Execution Order

Middleware is executed in the order added to the list, but the pipeline chains them in reverse (last added runs first, wrapping the next).

In the example, ErrorHandlingMiddleware runs first (outermost), then ProcessingMiddleware, then ValidationMiddleware (innermost).

## Error Handling

Use `ErrorHandlingMiddleware<T>` to catch exceptions and add them to `ctx.Errors`. Ensure your context inherits from `IngestionContext`.

## Full Data Ingestion Pipeline

For comprehensive data ingestion, use the middleware pipeline with specialized components:

```csharp
using AIKit.Core.Ingestion;
using AIKit.Core.Ingestion.Middleware;

// Define context
var context = new DataIngestionContext
{
    Files = new[] { "document1.txt", "document2.txt" }
};

// Build pipeline
var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
    .UseMiddleware<ErrorHandlingMiddleware<DataIngestionContext>>()
    .UseMiddleware<ReaderMiddleware>()
    .UseMiddleware<DocumentProcessorMiddleware>()
    .UseMiddleware<ChunkingMiddleware>()
    .UseMiddleware<WriterMiddleware>()
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

This pipeline reads files, processes the document, chunks it, and writes to a vector store.

## Integration with Vector Stores

The `WriterMiddleware` can be extended to integrate with AIKit's vector stores for storing chunks with embeddings. For example, use `AIKit.VectorStores.SqliteVec` or other providers to persist processed data for retrieval-augmented generation (RAG) workflows.

## Extensibility

The middleware pattern allows easy extension:

- Implement custom `IIngestionMiddleware<DataIngestionContext>` for specific processing steps.
- Override chunking logic in `ChunkingMiddleware` for semantic or token-based splitting.
- Add AI enrichments (e.g., summarization) by integrating with AIKit clients in document or chunk processors