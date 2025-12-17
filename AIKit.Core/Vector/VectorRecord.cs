namespace AIKit.Core.Vector;

public record VectorRecord(
    string VectorId,
    string DocumentId,
    float[] Embedding,
    IDictionary<string, object>? Properties = null
);
