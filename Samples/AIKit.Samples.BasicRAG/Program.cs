using AIKit.Clients.GitHub;
using AIKit.DataIngestion;
using AIKit.DataIngestion.Services.Chunking;
using AIKit.VectorStores.InMemory;
using AIKit.VectorStores.Search;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text;

public class VectorRecord
{
    [Microsoft.Extensions.VectorData.VectorStoreKey]
    public required string Key { get; set; }

    [Microsoft.Extensions.VectorData.VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Vector { get; set; }

    public required Dictionary<string, object?> Metadata { get; set; }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load();

        var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new Exception("Please provide a valid github token");

        // Configure OpenTelemetry with console exporter for AIKit telemetry
        using var tracerProvider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()

            .AddAIKitTracing()
            .AddConsoleExporter()
            .Build();

        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddAIKitMetrics()
            .AddConsoleExporter()
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

        // 1. Set up AI client
        var chatClient = new AIKit.Clients.GitHub.ChatClientBuilder()
            .WithGitHubToken(gitHubToken)
            .WithModel("gpt-4o-mini")
            .Build();

        // 2. Set up embedding generator
        var embeddingGenerator = new EmbeddingGeneratorBuilder()
            .WithGitHubToken(gitHubToken)
            .WithModel("text-embedding-3-small")
            .Build();

        // 3. Create vector store
        var builder = new AIKit.VectorStores.InMemory.VectorStoreBuilder();
        var vectorStore = builder.Build();
        var collection = vectorStore.GetCollection<string, VectorRecord>("My-vectors");
        await collection.EnsureCollectionExistsAsync();

        // 4. Build ingestion pipeline
        var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        var chunkingStrategy = new SectionBasedChunkingStrategy(tokenizer, new ChunkingOptions
        {
            MaxTokensPerChunk = 100,
            OverlapTokens = 10
        });

        var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
            .WithTelemetry(new TelemetryOptions { Enabled = true, ServiceName = "BasicRAGSample" })
            .WithLoggerFactory(loggerFactory)
            .Use(next => async (ctx, ct) =>
            {
                Console.WriteLine("Starting ingestion...");

                var content = """
# AIKit Overview

AIKit is a comprehensive .NET library built for .NET 10 that simplifies integrating AI into your applications.

## Key Capabilities

- Chatbots
- RAG (Retrieval-Augmented Generation)
- AI-powered developer tools

AIKit provides a unified API for multiple AI providers, vector databases, and storage solutions—without vendor lock-in.
""";


                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                var document = await new MarkdownReader().ReadAsync(stream, "document.md", "text/markdown");

                ctx.Documents.Add(document);

                await next(ctx, ct);
                return;
            })
            .Use(next => async (ctx, ct) =>
            {
                Console.WriteLine("Step 2: Chunking data...");

                foreach (var document in ctx.Documents)
                {
                    var chunks = await chunkingStrategy.Chunk(document);
                    ctx.DocumentChunks[document.Identifier] = chunks;
                }

                await next(ctx, ct);
                return;
            })
            .Use(next => async (ctx, ct) =>
            {
                Console.WriteLine("Step 3: Converting to vectors...");

                foreach (var kvp in ctx.DocumentChunks)
                {
                    var documentId = kvp.Key;
                    var chunks = kvp.Value;

                    foreach (var chunk in chunks)
                    {
                        var embedding = await embeddingGenerator.GenerateAsync(chunk.Content);
                        var record = new VectorRecord
                        {
                            Key = Guid.NewGuid().ToString(),
                            Vector = embedding.Vector,
                            Metadata = new Dictionary<string, object?> { ["content"] = chunk.Content, ["documentId"] = documentId }
                        };

                        await collection.UpsertAsync(record);
                    }
                }

                await next(ctx, ct);
                return;
            })
           .Build();

        // 5. Ingest data
        await pipeline.ExecuteAsync(new DataIngestionContext());

        //await pipeline.ExecuteAsync(new IngestionContext
        //{
        //    DocumentId = "doc1",
        //    Content = @"
        //AIKit is a comprehensive .NET library built for .NET 10 that simplifies integrating AI into your applications. 
        //Whether you're building chatbots, RAG (Retrieval-Augmented Generation) systems, or AI-powered tools, 
        //AIKit provides a unified API to interact with multiple AI providers, vector databases, storage solutions, 
        //and more—without vendor lock-in. Designed for .NET developers, it abstracts complexities so you can focus on your app's logic.
        //"
        //}, CancellationToken.None);

        // 6. Query with RAG
        while (true)
        {
            Console.Write("Enter your question (or 'exit' to quit): ");
            string? question = Console.ReadLine();
            if (string.IsNullOrEmpty(question) || question == "exit")
                break;

            var queryEmbedding = await embeddingGenerator.GenerateAsync(question);

            var contextBuilder = new System.Text.StringBuilder();
            var search = (IVectorStoreSearch<VectorRecord>)collection;
            await foreach (var r in search.VectorSearchAsync(queryEmbedding.Vector, new VectorSearchRequest<VectorRecord> { Top = 3 }))
            {
                var content = r.Record.Metadata["content"]?.ToString();
                contextBuilder.AppendLine(content);
            }
            var context = contextBuilder.ToString().Trim();
            var prompt = $"Context: {context}\nQuestion: {question}";

            var answer = await chatClient.GetResponseAsync(prompt);

            Console.WriteLine($"Answer: {answer.Text}");
        }
    }
}
