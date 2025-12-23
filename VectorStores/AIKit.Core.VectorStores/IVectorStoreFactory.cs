using Microsoft.Extensions.VectorData;

namespace AIKit.Core.VectorStores;

public interface IVectorStoreFactory
{
    string Provider { get; }

    VectorStore Create();
}

