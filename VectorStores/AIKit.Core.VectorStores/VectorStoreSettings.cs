using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AIKit.Core.Vector;

public sealed class VectorStoreSettings
{
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TenantId { get; set; }


    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? Schema { get; set; }
    public string TableName { get; set; }

    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    public int? MongoConnectionTimeoutSeconds { get; set; }
    public int? MongoMaxConnectionPoolSize { get; set; }
    public bool? MongoUseSsl { get; set; }

    public IEmbeddingGenerator? EmbeddingGenerator { get; internal set; }
}
