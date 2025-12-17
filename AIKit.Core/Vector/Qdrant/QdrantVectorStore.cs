using AIKit.Core.Vector;
using Google.Protobuf.Collections;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace AIKit.Core.Vector.Qdrant;

public sealed class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;

    public QdrantVectorStore(QdrantClient client, string collectionName)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _collectionName = !string.IsNullOrWhiteSpace(collectionName)
            ? collectionName
            : throw new ArgumentException("Collection name must not be empty.", nameof(collectionName));
    }

    public async Task UpsertAsync(VectorRecord record)
    {
        if (record is null) throw new ArgumentNullException(nameof(record));
        if (record.Embedding is null || record.Embedding.Length == 0)
            throw new ArgumentException("Embedding must not be null or empty.", nameof(record));
        if (string.IsNullOrWhiteSpace(record.DocumentId))
            throw new ArgumentException("DocumentId must not be null or empty.", nameof(record));

        var vectorId = string.IsNullOrWhiteSpace(record.VectorId)
            ? Guid.NewGuid().ToString("D")
            : record.VectorId;

        var point = new PointStruct
        {
            Id = new PointId { Uuid = vectorId },
            Vectors = record.Embedding,
            Payload =
            {
                ["documentId"] = record.DocumentId
            }
        };

        if (record.Properties is not null)
        {
            foreach (var (key, value) in record.Properties)
            {
                if (string.Equals(key, "documentId", StringComparison.OrdinalIgnoreCase))
                    continue;

                QdrantPayloadHelper.AddToPayload(point.Payload, key, value);
            }
        }

        await _client.UpsertAsync(_collectionName, new List<PointStruct> { point });
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("DocumentId must not be null or empty.", nameof(documentId));

        await _client.DeleteAsync(
                collectionName: _collectionName,
                filter: MatchKeyword("documentId", documentId));
    }
}

internal static class QdrantPayloadHelper
{
    public static void AddToPayload(
        MapField<string, Value> payload,
        string key,
        object? value)
    {
        if (payload is null) throw new ArgumentNullException(nameof(payload));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must not be null or empty.", nameof(key));
        if (value is null) return;

        switch (value)
        {
            case string s:
                payload[key] = s;
                break;

            case int i:
                payload[key] = i;
                break;

            case long l:
                payload[key] = l;
                break;

            case float f:
                payload[key] = f;
                break;

            case double d:
                payload[key] = d;
                break;

            case bool b:
                payload[key] = b;
                break;

            case Guid g:
                payload[key] = g.ToString();
                break;

            case DateTime dt:
                payload[key] = dt.ToString("O");
                break;

            case DateTimeOffset dto:
                payload[key] = dto.ToString("O");
                break;

            default:
                // Fallback: store string representation
                payload[key] = value.ToString() ?? string.Empty;
                break;
        }
    }
}
