using AIKit.Core.Vector;
using Npgsql;
using NpgsqlTypes;
using System.Text;

namespace AIKit.Core.Vector.PgVector;

public sealed class PgVectorStore : IVectorStore
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _recordsTable;
    private readonly string _propertiesTable;

    public PgVectorStore(
        NpgsqlDataSource dataSource,
        string recordsTable = "vector_records",
        string propertiesTable = "vector_properties")
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

        if (string.IsNullOrWhiteSpace(recordsTable))
            throw new ArgumentException("Records table name must not be null or empty.", nameof(recordsTable));
        if (string.IsNullOrWhiteSpace(propertiesTable))
            throw new ArgumentException("Properties table name must not be null or empty.", nameof(propertiesTable));

        _recordsTable = recordsTable;
        _propertiesTable = propertiesTable;
    }



    public async Task UpsertAsync(VectorRecord record)
    {
        if (record is null)
            throw new ArgumentNullException(nameof(record));
        if (record.Embedding is null || record.Embedding.Length == 0)
            throw new ArgumentException("Embedding must not be null or empty.", nameof(record));
        if (string.IsNullOrWhiteSpace(record.DocumentId))
            throw new ArgumentException("DocumentId must not be null or empty.", nameof(record));

        // If no VectorId is provided, generate one.
        // NOTE: The caller will not see this generated ID; if you need it upstream,
        // you might want to store it before calling or extend the interface.
        var vectorId = string.IsNullOrWhiteSpace(record.VectorId)
            ? Guid.NewGuid().ToString("D")
            : record.VectorId;

        await using var conn = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // 1) Upsert into vector_records
            var upsertSql = $@"
INSERT INTO {_recordsTable} (vector_id, document_id, embedding)
VALUES (@vector_id, @document_id, @embedding)
ON CONFLICT (vector_id) DO UPDATE SET
    document_id = EXCLUDED.document_id,
    embedding   = EXCLUDED.embedding;";

            await using (var upsertCmd = new NpgsqlCommand(upsertSql, conn, tx))
            {
                upsertCmd.Parameters.AddWithValue("vector_id", NpgsqlDbType.Text, vectorId);
                upsertCmd.Parameters.AddWithValue("document_id", NpgsqlDbType.Text, record.DocumentId);

                var embedding = new Pgvector.Vector(record.Embedding);
                upsertCmd.Parameters.AddWithValue("embedding", embedding);

                await upsertCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // 2) Clear old properties for this vector_id
            var deletePropsSql = $@"DELETE FROM {_propertiesTable} WHERE vector_id = @vector_id;";

            await using (var deleteCmd = new NpgsqlCommand(deletePropsSql, conn, tx))
            {
                deleteCmd.Parameters.AddWithValue("vector_id", NpgsqlDbType.Text, vectorId);
                await deleteCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // 3) Insert new properties (if any)
            if (record.Properties is not null && record.Properties.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {_propertiesTable} (vector_id, key, value) VALUES ");

                await using (var propsCmd = new NpgsqlCommand { Connection = conn, Transaction = tx })
                {
                    propsCmd.Parameters.AddWithValue("vector_id", NpgsqlDbType.Text, vectorId);

                    var index = 0;
                    foreach (var (key, value) in record.Properties)
                    {
                        if (index > 0)
                            sb.Append(", ");

                        var keyParam = $"key{index}";
                        var valueParam = $"value{index}";

                        sb.Append($"(@vector_id, @{keyParam}, @{valueParam})");

                        propsCmd.Parameters.AddWithValue(keyParam, NpgsqlDbType.Text, key);
                        propsCmd.Parameters.AddWithValue(
                            valueParam,
                            NpgsqlDbType.Text,
                            VectorPropertyValueConverter.ToString(value));

                        index++;
                    }

                    propsCmd.CommandText = sb.ToString();
                    await propsCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            await tx.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
            throw new ArgumentException("DocumentId must not be null or empty.", nameof(documentId));

        await using var conn = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            // 1) Delete properties for all vectors that belong to this document
            var deletePropsSql = $@"
            DELETE FROM {_propertiesTable}
            WHERE vector_id IN (
                SELECT vector_id FROM {_recordsTable} WHERE document_id = @document_id
            );";

            await using (var cmd = new NpgsqlCommand(deletePropsSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("document_id", NpgsqlDbType.Text, documentId);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // 2) Delete vectors
            var deleteVectorsSql = $@"
            DELETE FROM {_recordsTable}
            WHERE document_id = @document_id;";

            await using (var cmd = new NpgsqlCommand(deleteVectorsSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("document_id", NpgsqlDbType.Text, documentId);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            await tx.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

}


internal static class VectorPropertyValueConverter
{
    public static string ToString(object? value)
    {
        if (value is null) return string.Empty;

        return value switch
        {
            string s => s,
            int i => i.ToString(),
            long l => l.ToString(),
            float f => f.ToString("R"),   // round-trip
            double d => d.ToString("R"),
            decimal m => m.ToString(),
            bool b => b ? "true" : "false",
            Guid g => g.ToString("D"),
            DateTime dt => dt.ToUniversalTime().ToString("O"),
            DateTimeOffset o => o.ToUniversalTime().ToString("O"),
            _ => value.ToString() ?? string.Empty
        };
    }
}


public sealed class PgVectorSchemaInitializer
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _recordsTable;
    private readonly string _propertiesTable;
    private readonly int _embeddingDimensions;

    public PgVectorSchemaInitializer(
        NpgsqlDataSource dataSource,
        int embeddingDimensions,
        string recordsTable = "vector_records",
        string propertiesTable = "vector_properties")
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

        if (embeddingDimensions <= 0)
            throw new ArgumentOutOfRangeException(nameof(embeddingDimensions), "Embedding dimensions must be positive.");

        if (string.IsNullOrWhiteSpace(recordsTable))
            throw new ArgumentException("Records table name must not be null or empty.", nameof(recordsTable));
        if (string.IsNullOrWhiteSpace(propertiesTable))
            throw new ArgumentException("Properties table name must not be null or empty.", nameof(propertiesTable));

        _embeddingDimensions = embeddingDimensions;
        _recordsTable = recordsTable;
        _propertiesTable = propertiesTable;
    }

    /// <summary>
    /// Ensures the pgvector extension, tables and indexes exist.
    /// Safe to call multiple times (DDL uses IF NOT EXISTS).
    /// </summary>
    public async Task EnsureSchemaAsync(CancellationToken cancellationToken = default)
    {
        // Basic safety: these come from config/code, not user input.
        var recordsTable = _recordsTable;
        var propertiesTable = _propertiesTable;
        var dim = _embeddingDimensions;

        var ddl = $@"
-- 1) Ensure pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- 2) Core vector table
CREATE TABLE IF NOT EXISTS {recordsTable} (
    vector_id   text PRIMARY KEY,
    document_id text NOT NULL,
    embedding   vector({dim}) NOT NULL
);

-- 3) Properties table (EAV style)
CREATE TABLE IF NOT EXISTS {propertiesTable} (
    vector_id text NOT NULL REFERENCES {recordsTable}(vector_id) ON DELETE CASCADE,
    key       text NOT NULL,
    value     text NULL,
    PRIMARY KEY (vector_id, key)
);

-- 4) Helpful indexes
CREATE INDEX IF NOT EXISTS ix_{recordsTable}_document_id
    ON {recordsTable} (document_id);

CREATE INDEX IF NOT EXISTS ix_{propertiesTable}_key_value
    ON {propertiesTable} (key, value);
";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new NpgsqlCommand(ddl, conn);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}