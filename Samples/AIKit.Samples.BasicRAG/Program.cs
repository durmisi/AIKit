using AIKit.Clients.GitHub;
using AIKit.DataIngestion;
using AIKit.DataIngestion.Services.Chunking;
using AIKit.VectorStores.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;

var gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new Exception("Please provide a valid github token");

// 1. Set up AI client
var chatClient = new AIKit.Clients.GitHub.ChatClientBuilder()
    .WithGitHubToken(gitHubToken)
    .WithModel("gpt-4o-mini")
    .Build();

// 2. Set up embedding generator
var embeddingGenerator = new EmbeddingGeneratorBuilder()
    .WithGitHubToken(gitHubToken)
    .WithModel("text-embedding-ada-002")
    .Build();

// 3. Create vector store
var vectorStore = new VectorStoreBuilder().Build();

await vectorStore.CollectionExistsAsync("My-vectors");

// 4. Build ingestion pipeline
var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
var chunkingStrategy = new SectionBasedChunkingStrategy(tokenizer, new ChunkingOptions
{
    MaxTokensPerChunk = 100,
    OverlapTokens = 10
});

var pipeline = new IngestionPipelineBuilder<DataIngestionContext>()
    .Use(next => async (ctx, ct) =>
    {
        Console.WriteLine("Starting ingestion...");

        var markDownReader = new MarkdownReader();
        var doc1 = await markDownReader.ReadAsync(new FileInfo(""), "Document1");

        ctx.Documents.Add(doc1);

        await next(ctx, ct);
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
    })
    .Use(next => async (ctx, ct) =>
    {
        Console.WriteLine("Step 3: Converting to vectors...");
        await next(ctx, ct);
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
var queryEmbedding = await embeddingGenerator.GenerateAsync("What is AIKit?");

//foreach (var embedding in queryEmbeddings)
//{
//    Console.WriteLine($"Vector length: {embedding.Vector.Length}");
//}

//var searchResults = await vectorStore.SearchAsync(queryEmbedding.Vector, 
//    new VectorSearchOptions { Top = 3 });


////var context = string.Join("\n", searchResults.Select(r => r.Record.Metadata["content"]));
////var prompt = $"Context: {context}\nQuestion: What is AIKit?";
////var answer = await chatClient.GetResponseAsync(prompt);
