using AIKit.Clients.GitHub;
using AIKit.VectorStores.InMemory;

// 1. Set up AI client
var chatClient = new AIKit.Clients.GitHub.ChatClientBuilder()
    .WithGitHubToken("your-github-token")
    .WithModel("gpt-4o-mini")
    .Build();

// 2. Set up embedding generator
var embeddingGenerator = new EmbeddingGeneratorBuilder()
    .WithGitHubToken("your-github-token")
    .WithModel("text-embedding-ada-002")
    .Build();

// 3. Create vector store
var vectorStore = new VectorStoreBuilder()
    .Build();

//// 4. Build ingestion pipeline
//var pipeline = new IngestionPipelineBuilder<string>()
//     .Use(next => (ctx, ct) =>
//     {

//         throw new NotImplementedException();
//     })
//    .Build();

////// 5. Ingest data
//await pipeline.ExecuteAsync(@"

//AIKit is a comprehensive .NET library built for .NET 10 that simplifies integrating AI into your applications. 
//Whether you're building chatbots, RAG (Retrieval-Augmented Generation) systems, or AI-powered tools, 
//AIKit provides a unified API to interact with multiple AI providers, vector databases, storage solutions, 
//and more—without vendor lock-in. Designed for .NET developers, it abstracts complexities so you can focus on your app's logic.

//");

////// 6. Query with RAG
//var texts = new List<string> { "What is AIKit?", "AI embeddings are cool" };

//var queryEmbeddings = await embeddingGenerator.GenerateAsync(texts, new EmbeddingGenerationOptions
//{

//});

//foreach (var embedding in queryEmbeddings)
//{
//    Console.WriteLine($"Vector length: {embedding.Vector.Length}");
//}

////var searchResults = await vectorStore.SearchAsync(queryEmbedding.Vector, new VectorSearchOptions { Top = 3 });
////var context = string.Join("\n", searchResults.Select(r => r.Record.Metadata["content"]));
////var prompt = $"Context: {context}\nQuestion: What is AIKit?";
////var answer = await chatClient.GetResponseAsync(prompt);