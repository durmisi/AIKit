Perfect — now let’s make this **concrete and usable end-to-end**.

Below is the **exact, minimal flow** for using `VectorSearchResult<T>` in your AIKit architecture, **without leaking provider details**, and **fully aligned with Semantic Kernel**.

---

# 🧩 End-to-End Usage Flow

We’ll assume you already have:

✔ a registered `IVectorStoreFactory`
✔ a `VectorStoreFactoryResolver`
✔ a `VectorStoreSearchResolver`
✔ an embedding generator

---

## 1️⃣ Define a Record Model

```csharp
using Microsoft.Extensions.VectorData;

public sealed class DocumentRecord
{
    [VectorStoreKey]
    public string Id { get; init; } = default!;

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Content { get; init; } = default!;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> ContentEmbedding { get; init; }
}
```

---

## 2️⃣ Create the Vector Store (via Factory Resolver)

```csharp
var vectorStore = vectorStoreFactoryResolver
    .Resolve("azure-ai-search");
```

> This gives you a `VectorStore`, **not a collection yet**.

---

## 3️⃣ Get or Create a Collection

```csharp
var collection = vectorStore
    .GetCollection<string, DocumentRecord>("documents");

// Optional: create schema if needed
await collection.CreateIfNotExistsAsync();
```

---

## 4️⃣ Resolve the Search Adapter

```csharp
var search = vectorStoreSearchResolver
    .Resolve<DocumentRecord>(vectorStore, "documents");
```

This returns:

```csharp
IVectorStoreSearch<DocumentRecord>
```

---

## 5️⃣ Generate an Embedding for the Query

```csharp
ReadOnlyMemory<float> queryVector =
    await embeddingGenerator.GenerateEmbeddingAsync(
        "How do I configure vector search?");
```

---

## 6️⃣ Run Vector Search

```csharp
var request = new VectorSearchRequest<DocumentRecord>
{
    Top = 5,
    IncludeVectors = false,
    Filter = d => d.Content.Length > 100
};

await foreach (VectorSearchResult<DocumentRecord> result
    in search.VectorSearchAsync(queryVector, request))
{
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine($"Text: {result.Record.Content}");
}
```

### 🔍 What you get per result

```csharp
result.Record   // Your DocumentRecord
result.Score    // Similarity score
result.Metadata // Optional provider metadata
```

---

## 7️⃣ Run Hybrid Search (If Supported)

```csharp
var hybridRequest = new HybridSearchRequest<DocumentRecord>
{
    Top = 5,
    TextProperty = d => d.Content
};

await foreach (var result in search.HybridSearchAsync(
    queryVector,
    keywords: new[] { "vector", "search", "configuration" },
    hybridRequest))
{
    Console.WriteLine($"{result.Score}: {result.Record.Content}");
}
```

If the provider **does not support hybrid search**, your adapter throws:

```text
NotSupportedException: Hybrid search not supported.
```

✔ Correct behavior
✔ No fake implementations

---

# 🧠 Important Notes

### ✔ Streaming by design

`IAsyncEnumerable<VectorSearchResult<T>>` means:

* Results arrive progressively
* Works well with large datasets
* No buffering required

---

### ✔ No Provider Knowledge Required

Your application code:

* Doesn’t know Azure AI Search
* Doesn’t know Qdrant
* Doesn’t know index types (HNSW, IVF, etc.)

Those live in:

* Factory
* Builder
* Options

---

## 🚫 Common Mistakes to Avoid

❌ Wrapping `VectorSearchResult<T>`
❌ Returning `List<T>`
❌ Baking search into factories
❌ Creating one “mega search interface”

---

# ✅ Final Architecture Recap

```text
[App Code]
   ↓
IVectorStoreFactoryResolver
   ↓
VectorStore
   ↓
VectorStoreSearchResolver
   ↓
IVectorStoreSearch<T>
   ↓
VectorSearchResult<T>
```

---

## 🚀 Where You Can Extend Next

* 🔁 Hybrid → Vector fallback automatically
* 🎯 Reranking layer
* 🔐 Security filtering
* 📊 Telemetry hooks
* 🧠 Query pipelines

If you want, I can implement **automatic capability detection + fallback** next — just say it.
