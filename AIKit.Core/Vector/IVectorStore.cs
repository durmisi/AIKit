namespace AIKit.Core.Vector;

public interface IVectorStore
{
    Task UpsertAsync(VectorRecord record);
    Task DeleteDocumentAsync(string documentId);
}
