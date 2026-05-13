using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.AI.VectorStore;
using Polly;
using Polly.Registry;

namespace VK.Blocks.AI.VectorStore.Sqlite.Internal;

/// <summary>
/// SQLite implementation of <see cref="IVKAIVectorDatabase"/> using the sqlite-vec extension.
/// Stores vectors in a vec0 virtual table and performs native KNN search.
/// </summary>
internal sealed class AIVectorStoreSqliteDatabase : IVKAIVectorDatabase
{
    private readonly VKAIVectorStoreSqliteOptions _options;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly ILogger<AIVectorStoreSqliteDatabase> _logger;
    private readonly ResiliencePipeline _pipeline;
    private bool _initialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public AIVectorStoreSqliteDatabase(
        IOptions<VKAIVectorStoreSqliteOptions> options,
        IVKJsonSerializer jsonSerializer,
        ILogger<AIVectorStoreSqliteDatabase> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _options = VKGuard.NotNull(options.Value); // [AP.01]
        _jsonSerializer = VKGuard.NotNull(jsonSerializer); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]
        _pipeline = pipelineProvider.GetPipeline("AI.VectorStore.Sqlite");
    }

    private async Task<SqliteConnection> GetOpenConnectionAsync(CancellationToken ct)
    {
        var connection = new SqliteConnection(_options.Connection);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        try
        {
            // Load the sqlite-vec extension (provided by Microsoft.SemanticKernel.Connectors.SqliteVec)
            connection.LoadExtension("sqlite-vec");
        }
        catch (SqliteException ex)
        {
            _logger.ExtensionLoadFailed(ex);
        }

        return connection;
    }

    public async Task<VKResult> UpsertAsync(string id, VKEmbeddingVector vector, string content, VKAIVectorMetadata metadata, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(id); // [AP.01]
        VKGuard.NotNull(vector); // [AP.01]
        VKGuard.NotNull(metadata); // [AP.01]

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

        using var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Upsert Metadata and get ROWID
            const string metaSql = @"
                INSERT INTO VK_AI_Metadata (Id, TenantId, Content, Metadata)
                VALUES (@id, @tenantId, @content, @metadata)
                ON CONFLICT(Id) DO UPDATE SET
                    TenantId = excluded.TenantId,
                    Content = excluded.Content,
                    Metadata = excluded.Metadata
                RETURNING ROWID;";

            using var metaCmd = new SqliteCommand(metaSql, connection, transaction);
            metaCmd.Parameters.AddWithValue("@id", id);
            metaCmd.Parameters.AddWithValue("@tenantId", metadata.TenantId);
            metaCmd.Parameters.AddWithValue("@content", content);
            metaCmd.Parameters.AddWithValue("@metadata", _jsonSerializer.Serialize(metadata));

            var rowId = (long?)await _pipeline.ExecuteAsync(async ct => await metaCmd.ExecuteScalarAsync(ct), cancellationToken).ConfigureAwait(false);
            if (rowId == null)
                return VKResult.Failure(Errors.Database.ExecutionFailed);

            // 2. Upsert Vector using the same ROWID
            const string vecSql = @"
                INSERT INTO VK_AI_Vectors (rowid, embedding)
                VALUES (@rowid, @embedding)
                ON CONFLICT(rowid) DO UPDATE SET
                    embedding = excluded.embedding;";

            using var vecCmd = new SqliteCommand(vecSql, connection, transaction);
            vecCmd.Parameters.AddWithValue("@rowid", rowId);
            vecCmd.Parameters.AddWithValue("@embedding", MemoryMarshal.AsBytes(vector.Values.Span).ToArray());

            await _pipeline.ExecuteAsync(async ct => await vecCmd.ExecuteNonQueryAsync(ct), cancellationToken).ConfigureAwait(false);

            transaction.Commit();
        }
        catch (SqliteException ex)
        {
            transaction.Rollback();
            _logger.CommandFailed(ex, "Upsert (Metadata + Vector)");
            return VKResult.Failure(Errors.Database.ExecutionFailed);
        }

        return VKResult.Success();
    }

    public async Task<VKResult<IEnumerable<VKAIVectorRecord>>> SearchAsync(VKEmbeddingVector vector, VKAIVectorSearchArgs args, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(vector); // [AP.01]
        VKGuard.NotNull(args); // [AP.01]

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        using var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // Native sqlite-vec KNN search using MATCH
        const string sql = @"
            SELECT
                m.Id,
                m.Content,
                m.Metadata,
                v.distance
            FROM VK_AI_Vectors v
            JOIN VK_AI_Metadata m ON v.rowid = m.rowid
            WHERE m.TenantId = @tenantId
              AND v.embedding MATCH @embedding
              AND k = @limit
            ORDER BY v.distance;";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@tenantId", args.TenantId);
        command.Parameters.AddWithValue("@embedding", MemoryMarshal.AsBytes(vector.Values.Span).ToArray());
        command.Parameters.AddWithValue("@limit", args.Limit);

        var results = new List<VKAIVectorRecord>();
        try
        {
            using var reader = await _pipeline.ExecuteAsync(async ct => await command.ExecuteReaderAsync(ct), cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var score = 1.0f - (float)reader.GetDouble(3); // Convert distance to similarity score

                if (score >= args.MinScore)
                {
                    var metadataJson = reader.GetString(2);
                    var metadata = _jsonSerializer.Deserialize<VKAIVectorMetadata>(metadataJson);

                    results.Add(new VKAIVectorRecord
                    {
                        Id = reader.GetString(0),
                        Content = reader.GetString(1),
                        Metadata = metadata ?? new VKAIVectorMetadata { TenantId = args.TenantId },
                        Score = score
                    });
                }
            }
        }
        catch (SqliteException ex)
        {
            _logger.CommandFailed(ex, sql);
            return VKResult.Failure<IEnumerable<VKAIVectorRecord>>(Errors.Database.ExecutionFailed);
        }

        return VKResult.Success(results.AsEnumerable());
    }

    public async Task<VKResult> DeleteAsync(string tenantId, string id, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(tenantId);
        VKGuard.NotNullOrWhiteSpace(id);

        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

        using var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Get ROWID first
            const string getSql = "SELECT ROWID FROM VK_AI_Metadata WHERE Id = @id AND TenantId = @tenantId";
            using var getCmd = new SqliteCommand(getSql, connection, transaction);
            getCmd.Parameters.AddWithValue("@id", id);
            getCmd.Parameters.AddWithValue("@tenantId", tenantId);

            var rowId = (long?)await _pipeline.ExecuteAsync(async ct => await getCmd.ExecuteScalarAsync(ct), cancellationToken).ConfigureAwait(false);

            if (rowId != null)
            {
                // 2. Delete from both
                const string delMetaSql = "DELETE FROM VK_AI_Metadata WHERE ROWID = @rowid";
                using var delMetaCmd = new SqliteCommand(delMetaSql, connection, transaction);
                delMetaCmd.Parameters.AddWithValue("@rowid", rowId);
                await _pipeline.ExecuteAsync(async ct => await delMetaCmd.ExecuteNonQueryAsync(ct), cancellationToken).ConfigureAwait(false);

                const string delVecSql = "DELETE FROM VK_AI_Vectors WHERE rowid = @rowid";
                using var delVecCmd = new SqliteCommand(delVecSql, connection, transaction);
                delVecCmd.Parameters.AddWithValue("@rowid", rowId);
                await _pipeline.ExecuteAsync(async ct => await delVecCmd.ExecuteNonQueryAsync(ct), cancellationToken).ConfigureAwait(false);
            }

            transaction.Commit();
        }
        catch (SqliteException ex)
        {
            transaction.Rollback();
            _logger.CommandFailed(ex, "Delete");
            return VKResult.Failure(Errors.Database.ExecutionFailed);
        }

        return VKResult.Success();
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_initialized)
                return;

            using var connection = await GetOpenConnectionAsync(ct).ConfigureAwait(false);

            // 1. Create Metadata Table
            const string metaSql = @"
                CREATE TABLE IF NOT EXISTS VK_AI_Metadata (
                    Id TEXT PRIMARY KEY,
                    TenantId TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    Metadata TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_VK_AI_Metadata_TenantId ON VK_AI_Metadata(TenantId);";

            using var metaCmd = new SqliteCommand(metaSql, connection);
            await _pipeline.ExecuteAsync(async ct => await metaCmd.ExecuteNonQueryAsync(ct), ct).ConfigureAwait(false);

            // 2. Create Virtual Vector Table (vec0)
            // Note: dim is fixed at table creation time in sqlite-vec
            string vecSql = @$"
                CREATE VIRTUAL TABLE IF NOT EXISTS VK_AI_Vectors USING vec0(
                    embedding float[{_options.EmbeddingDimension}]
                );";

            using var vecCmd = new SqliteCommand(vecSql, connection);
            await _pipeline.ExecuteAsync(async ct => await vecCmd.ExecuteNonQueryAsync(ct), ct).ConfigureAwait(false);

            _logger.DatabaseInitialized(_options.Connection);
            _initialized = true;
        }
        catch (SqliteException ex)
        {
            _logger.CommandFailed(ex, "Table Creation (sqlite-vec)");
            throw; // Critical initialization failure
        }
        finally
        {
            _initLock.Release();
        }
    }

    // Cosine similarity calculation is now performed natively by sqlite-vec
}
