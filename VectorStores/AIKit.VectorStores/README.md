# AIKit.VectorStores

[![NuGet Version](https://img.shields.io/nuget/v/AIKit.VectorStores.svg)](https://www.nuget.org/packages/AIKit.VectorStores/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AIKit.VectorStores.svg)](https://www.nuget.org/packages/AIKit.VectorStores/)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

## Description

Vector store abstractions and common interfaces

## Installation

```bash
dotnet add package AIKit.VectorStores --version 1.0.0
```

## Usage Example

```csharp
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
```

## API Reference

For detailed API documentation, see [API Reference](https://github.com/durmisi/AIKit/docs/api/AIKit.VectorStores).

## Links

- [GitHub Repository](https://github.com/durmisi/AIKit)
- [Issues](https://github.com/durmisi/AIKit/issues)
- [License](https://github.com/durmisi/AIKit/blob/main/LICENSE)
