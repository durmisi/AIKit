# AIKit NuGet Packages

AIKit is a comprehensive .NET library for AI integration, distributed as modular NuGet packages. This allows you to install only the components you need for your project, keeping your dependencies minimal.

## Package Categories

AIKit packages are organized into the following categories:

- **Core Packages**: Essential abstractions and interfaces
- **AI Clients**: Provider-specific implementations for chat and embeddings
- **Vector Stores**: Integrations for vector databases and similarity search
- **Storage**: File storage abstractions with versioning
- **Ingestion**: Data processing and indexing pipelines
- **Prompts**: Templating engines for dynamic prompt generation

## Core Packages

These packages provide the foundational interfaces and abstractions used throughout AIKit.

| Package              | Description                                                                                   | Installation                                            |
| -------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| AIKit.Core.Clients   | Core interfaces and settings for AI client providers (IChatClient, IEmbeddingGenerator, etc.) | dotnet add package AIKit.Core.Clients --version 1.0.0   |
| AIKit.VectorStores   | Vector store abstractions and common interfaces                                               | dotnet add package AIKit.VectorStores --version 1.0.0   |
| AIKit.Storage        | Storage abstractions for file management with versioning                                      | dotnet add package AIKit.Storage --version 1.0.0        |
| AIKit.Core.Ingestion | Data ingestion pipeline abstractions and chunking strategies                                  | dotnet add package AIKit.Core.Ingestion --version 1.0.0 |
| AIKit.Prompts        | Prompt templating abstractions and base classes                                               | dotnet add package AIKit.Prompts --version 1.0.0        |

## AI Clients

These packages provide implementations for various AI providers, supporting chat completion and text embeddings.

| Package                   | Provider            | Features         | Installation                                                 |
| ------------------------- | ------------------- | ---------------- | ------------------------------------------------------------ |
| AIKit.Clients.OpenAI      | OpenAI              | Chat, Embeddings | dotnet add package AIKit.Clients.OpenAI --version 1.0.0      |
| AIKit.Clients.AzureOpenAI | Azure OpenAI        | Chat, Embeddings | dotnet add package AIKit.Clients.AzureOpenAI --version 1.0.0 |
| AIKit.Clients.Claude      | Anthropic Claude    | Chat             | dotnet add package AIKit.Clients.Claude --version 1.0.0      |
| AIKit.Clients.AzureClaude | Azure-hosted Claude | Chat             | dotnet add package AIKit.Clients.AzureClaude --version 1.0.0 |
| AIKit.Clients.Gemini      | Google Gemini       | Chat, Embeddings | dotnet add package AIKit.Clients.Gemini --version 1.0.0      |
| AIKit.Clients.Bedrock     | AWS Bedrock         | Chat, Embeddings | dotnet add package AIKit.Clients.Bedrock --version 1.0.0     |
| AIKit.Clients.Ollama      | Local Ollama        | Chat             | dotnet add package AIKit.Clients.Ollama --version 1.0.0      |
| AIKit.Clients.GitHub      | GitHub Models       | Chat, Embeddings | dotnet add package AIKit.Clients.GitHub --version 1.0.0      |
| AIKit.Clients.Groq        | Groq                | Chat             | dotnet add package AIKit.Clients.Groq --version 1.0.0        |
| AIKit.Clients.Mistral     | Mistral AI          | Chat             | dotnet add package AIKit.Clients.Mistral --version 1.0.0     |

## Vector Stores

These packages provide implementations for various vector databases, enabling efficient similarity search and RAG applications.

| Package                          | Database                 | Description                                                | Installation                                                        |
| -------------------------------- | ------------------------ | ---------------------------------------------------------- | ------------------------------------------------------------------- |
| AIKit.VectorStores.InMemory      | In-memory                | Simple in-memory vector store for testing and development  | dotnet add package AIKit.VectorStores.InMemory --version 1.0.0      |
| AIKit.VectorStores.CosmosMongoDB | CosmosDB MongoDB API     | Vector storage using Azure CosmosDB with MongoDB API       | dotnet add package AIKit.VectorStores.CosmosMongoDB --version 1.0.0 |
| AIKit.VectorStores.CosmosNoSQL   | CosmosDB NoSQL API       | Vector storage using Azure CosmosDB with NoSQL API         | dotnet add package AIKit.VectorStores.CosmosNoSQL --version 1.0.0   |
| AIKit.VectorStores.Elasticsearch | Elasticsearch            | Vector storage and search using Elasticsearch              | dotnet add package AIKit.VectorStores.Elasticsearch --version 1.0.0 |
| AIKit.VectorStores.MongoDB       | MongoDB                  | Vector storage using MongoDB Atlas                         | dotnet add package AIKit.VectorStores.MongoDB --version 1.0.0       |
| AIKit.VectorStores.PgVector      | PostgreSQL with PgVector | Vector storage using PostgreSQL with PgVector extension    | dotnet add package AIKit.VectorStores.PgVector --version 1.0.0      |
| AIKit.VectorStores.Qdrant        | Qdrant                   | Vector storage using Qdrant vector database                | dotnet add package AIKit.VectorStores.Qdrant --version 1.0.0        |
| AIKit.VectorStores.Redis         | Redis                    | Vector storage using Redis with vector search capabilities | dotnet add package AIKit.VectorStores.Redis --version 1.0.0         |
| AIKit.VectorStores.SqliteVec     | SQLite with Vec          | Vector storage using SQLite with Vec extension             | dotnet add package AIKit.VectorStores.SqliteVec --version 1.0.0     |
| AIKit.VectorStores.SqlServer     | SQL Server               | Vector storage using Microsoft SQL Server                  | dotnet add package AIKit.VectorStores.SqlServer --version 1.0.0     |
| AIKit.VectorStores.Weaviate      | Weaviate                 | Vector storage using Weaviate vector database              | dotnet add package AIKit.VectorStores.Weaviate --version 1.0.0      |
| AIKit.VectorStores.AzureAISearch | Azure AI Search          | Vector storage using Azure AI Search service               | dotnet add package AIKit.VectorStores.AzureAISearch --version 1.0.0 |
| AIKit.VectorStores.Pinecone      | Pinecone                 | Vector storage using Pinecone vector database              | dotnet add package AIKit.VectorStores.Pinecone --version 1.0.0      |

## Storage

These packages provide file storage implementations with versioning support.

| Package             | Storage Type       | Description                                           | Installation                                           |
| ------------------- | ------------------ | ----------------------------------------------------- | ------------------------------------------------------ |
| AIKit.Storage.Azure | Azure Blob Storage | File storage with versioning using Azure Blob Storage | dotnet add package AIKit.Storage.Azure --version 1.0.0 |
| AIKit.Storage.Local | Local File System  | File storage with versioning using local file system  | dotnet add package AIKit.Storage.Local --version 1.0.0 |

## Prompts

These packages provide templating engines for dynamic prompt generation.

| Package                  | Engine     | Description                       | Installation                                                |
| ------------------------ | ---------- | --------------------------------- | ----------------------------------------------------------- |
| AIKit.Prompts.Handlebars | Handlebars | Handlebars templating for .NET    | dotnet add package AIKit.Prompts.Handlebars --version 1.0.0 |
| AIKit.Prompts.Jinja2     | Jinja2     | Jinja2 templating (Python-style)  | dotnet add package AIKit.Prompts.Jinja2 --version 1.0.0     |
| AIKit.Prompts.Liquid     | Liquid     | Liquid templating (Shopify-style) | dotnet add package AIKit.Prompts.Liquid --version 1.0.0     |

## Ingestion

| Package              | Description                                                                  | Installation                                            |
| -------------------- | ---------------------------------------------------------------------------- | ------------------------------------------------------- |
| AIKit.Core.Ingestion | Data ingestion pipelines for processing and indexing data into vector stores | dotnet add package AIKit.Core.Ingestion --version 1.0.0 |

## Usage Examples

### Basic AI Client Setup

`csharp
using AIKit.Clients.OpenAI;

// Create a chat client
var chatClient = new ChatClientBuilder()
.WithApiKey("your-openai-api-key")
.WithModel("gpt-4")
.Build();

// Get a response
var response = await chatClient.GetResponseAsync("Explain AIKit in simple terms.");
Console.WriteLine(response.Text);
`

### Vector Store Usage

`csharp
using AIKit.VectorStores.InMemory;
using Microsoft.Extensions.VectorData;

// Create a vector store
var vectorStore = new InMemoryVectorStore();

// Add a document with embedding
await vectorStore.AddAsync(new VectorDocument
{
Id = "doc1",
Vector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f }),
Metadata = new Dictionary<string, object> { ["title"] = "AIKit Intro" }
});

// Search for similar vectors
var results = await vectorStore.SearchAsync(
new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f }),
new VectorSearchOptions { Top = 5 });
`

### Ingestion Pipeline

`csharp
using AIKit.Core.Ingestion;
using AIKit.Clients.OpenAI;
using AIKit.VectorStores.InMemory;

// Set up components
var chatClient = new ChatClientBuilder().WithApiKey("key").Build();
var embeddingGenerator = new EmbeddingGeneratorBuilder()
.WithApiKey("key")
.WithModelId("text-embedding-ada-002")
.Build();
var vectorStore = new InMemoryVectorStore();

// Build ingestion pipeline
var pipeline = new IngestionPipelineBuilder<string>()
.WithChunking(new TokenChunkingStrategy(512))
.WithWriter(new VectorStoreWriter(vectorStore, chatClient))
.Build();

// Ingest data
await pipeline.ProcessAsync("Your large text document here.");

// Query with RAG
var queryEmbedding = await embeddingGenerator.GenerateAsync("What is AIKit?");
var searchResults = await vectorStore.SearchAsync(queryEmbedding.Vector, new VectorSearchOptions { Top = 3 });
var context = string.Join("\n", searchResults.Select(r => r.Record.Metadata["content"]));
var prompt = $"Context: {context}\nQuestion: What is AIKit?";
var answer = await chatClient.GetResponseAsync(prompt);
`

### Prompt Templating

`csharp
using AIKit.Prompts.Jinja2;

var template = "Hello {{name}}, explain {{topic}}.";
var executor = new Jinja2PromptExecutor(chatClient);
var renderedPrompt = executor.Render(template, new { name = "Developer", topic = "AI integration" });
var response = await executor.ExecuteAsync(renderedPrompt);
`

## Publishing Packages

To publish packages to NuGet.org:

1. Obtain an API key from https://www.nuget.org/account/apikeys
2. For each package: dotnet nuget push <path-to-nupkg> --api-key <your-key> --source https://api.nuget.org/v3/index.json

For private feeds, adjust the source URL accordingly.

## Versioning

All packages follow semantic versioning (SemVer). The current stable version is 1.0.0.

## Dependencies

AIKit packages are built on .NET 10.0 and leverage:

- Microsoft.Extensions.AI for AI abstractions
- Microsoft.Extensions.VectorData for vector operations
- Provider-specific SDKs (OpenAI, Azure, etc.)

## Support

- **Documentation**: See the main [README.md](README.md) for detailed usage guides
- **Issues**: Report bugs on [GitHub Issues](https://github.com/durmisi/AIKit/issues)
- **Contributing**: See [CONTRIBUTING.md](CONTRIBUTING.md)

---

## Deprecated Types ⚠️

- **BaseEmbeddingGeneratorFactory**: Deprecated — this base class will be removed in a future release. Implement IEmbeddingGeneratorBuilder directly in your provider factory and move constructor validation, Provider, Create()/Create(settings), and any RetryPolicy handling into the concrete implementation.

---
