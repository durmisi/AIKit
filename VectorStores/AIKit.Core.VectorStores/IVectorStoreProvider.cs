using Microsoft.Extensions.VectorData;

namespace AIKit.Core.VectorStores;

public interface IVectorStoreProvider
{
    string Provider { get; }

    VectorStore Create();

    VectorStore Create(VectorStoreSettings settings);
}
