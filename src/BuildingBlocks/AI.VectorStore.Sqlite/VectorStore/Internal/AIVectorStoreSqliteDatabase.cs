using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using VK.Blocks.AI;
using VK.Blocks.AI.VectorStore;
using VK.Blocks.AI.VectorStore.Sqlite;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.VectorStore.Internal;

/// <summary>
/// SQLite implementation of <see cref="IVKAIVectorStore"/> using the sqlite-vec extension.
/// Stores vectors in named vec0 virtual tables and metadata in corresponding shadow tables.
/// </summary>
internal sealed class AIVectorStoreSqliteDatabase : IVKAIVectorStore
{
    private readonly VKAIVectorStoreSqliteOptions _options;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly ILogger<AIVectorStoreSqliteDatabase> _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly HashSet<string> _initializedCollections = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public AIVectorStoreSqliteDatabase(
        IOptions<VKAIVectorStoreSqliteOptions> options,
        IVKJsonSerializer jsonSerializer,
        ILogger<AIVectorStoreSqliteDatabase> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        VKGuard.NotNull(options);
        _options = VKGuard.NotNull(options.Value); // [AP.01]
        _jsonSerializer = VKGuard.NotNull(jsonSerializer); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]
        _pipeline = VKGuard.NotNull(pipelineProvider).GetPipeline("AI.VectorStore.Sqlite");
    }

    public IVKAIVectorCollection<T> Collection<T>(string name) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(name);
        return new SqliteVectorCollection<T>(name, this);
    }

    internal async Task<VKResult> UpsertGenericAsync<T>(string collectionName, string id, T document, VKEmbeddingsVector vector, CancellationToken ct) where T : class
    {
        VKGuard.NotNullOrWhiteSpace(id);
        VKGuard.NotNull(document);
        VKGuard.NotNull(vector);

        await EnsureCollectionInitializedAsync(collectionName, ct).ConfigureAwait(false);

        using var connection = await GetOpenConnectionAsync(ct).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();

        var sw = Stopwatch.StartNew();
        try
        {
            var metaTable = GetMetadataTableName(collectionName);
            var vecTable = GetVectorTableName(collectionName);

            // 1. Upsert Metadata (serialized document)
            string metaSql = $@"
                INSERT INTO {metaTable} (Id, Data)
                VALUES (@id, @data)
                ON CONFLICT(Id) DO UPDATE SET Data = excluded.Data;
            ";

            using var metaCmd = new SqliteCommand(metaSql, connection, transaction);
            metaCmd.Parameters.AddWithValue("@id", id);
            metaCmd.Parameters.AddWithValue("@data", _jsonSerializer.Serialize(document));
            await _pipeline.ExecuteAsync(async ct => await metaCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            // 2. Upsert Vector (using sqlite-vec specific insertion)
            // Note: rowid in virtual table must match rowid in metadata table for join
            string getRowIdSql = $"SELECT ROWID FROM {metaTable} WHERE Id = @id";
            using var getRowIdCmd = new SqliteCommand(getRowIdSql, connection, transaction);
            getRowIdCmd.Parameters.AddWithValue("@id", id);
            var rowIdResult = await _pipeline.ExecuteAsync(async ct => await getRowIdCmd.ExecuteScalarAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);
            if (rowIdResult == null)
                return VKResult.Failure(Errors.Database.ExecutionFailed);

            var rowId = (long)rowIdResult;

            string vecSql = $@"
                INSERT INTO {vecTable} (rowid, vector)
                VALUES (@rowid, @vector)
                ON CONFLICT(rowid) DO UPDATE SET vector = excluded.vector;
            ";

            using var vecCmd = new SqliteCommand(vecSql, connection, transaction);
            vecCmd.Parameters.AddWithValue("@rowid", rowId);
            vecCmd.Parameters.AddWithValue("@vector", MemoryMarshal.AsBytes(vector.Values.Span).ToArray());
            await _pipeline.ExecuteAsync(async ct => await vecCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            transaction.Commit();
            VKAIVectorStoreSqliteDiagnostics.RecordUpsertDuration(sw.Elapsed.TotalSeconds);
        }
        catch (SqliteException ex)
        {
            transaction.Rollback();
            _logger.CommandFailed(ex, $"Upsert to {collectionName}");
            VKAIVectorStoreSqliteDiagnostics.RecordError();
            return VKResult.Failure(Errors.Database.ExecutionFailed);
        }

        return VKResult.Success();
    }

    internal async Task<VKResult<IEnumerable<VKAIVectorRecord<T>>>> SearchGenericAsync<T>(string collectionName, VKEmbeddingsVector vector, VKAIVectorSearchArgs args, CancellationToken ct) where T : class
    {
        VKGuard.NotNull(vector);
        VKGuard.NotNull(args);

        await EnsureCollectionInitializedAsync(collectionName, ct).ConfigureAwait(false);

        using var connection = await GetOpenConnectionAsync(ct).ConfigureAwait(false);

        var sw = Stopwatch.StartNew();
        try
        {
            var metaTable = GetMetadataTableName(collectionName);
            var vecTable = GetVectorTableName(collectionName);

            // sqlite-vec specific: vector_distance(v1, v2) for similarity
            // Using subquery or join to combine metadata and vectors
            string sql = $@"
                SELECT m.Id, m.Data, v.distance
                FROM {vecTable} v
                JOIN {metaTable} m ON v.rowid = m.ROWID
                WHERE vector_distance(v.vector, @query, 'cosine') < @threshold
                ORDER BY distance
                LIMIT @limit
            ";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@query", MemoryMarshal.AsBytes(vector.Values.Span).ToArray());

            // [AP.05] Hierarchical Configuration: Local Override > Global Default
            // Note: Since VKAIVectorSearchArgs has hardcoded defaults, we only override if they differ or we can define a "zero" value as "use default".
            // For now, we honor the passed args but ensure they are merged with block defaults if the pattern was deeper.
            var minScore = args.MinScore > 0 ? args.MinScore : _options.DefaultMinScore;
            var limit = args.Limit > 0 ? args.Limit : _options.DefaultSearchLimit;

            cmd.Parameters.AddWithValue("@threshold", 1.0f - minScore); // distance = 1 - similarity
            cmd.Parameters.AddWithValue("@limit", limit);

            var results = new List<VKAIVectorRecord<T>>();
            using var reader = await _pipeline.ExecuteAsync(async ct => await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var distance = reader.GetFloat(2);
                var score = 1.0f - distance;

                if (score >= args.MinScore)
                {
                    var dataJson = reader.GetString(1);
                    var document = _jsonSerializer.Deserialize<T>(dataJson);

                    if (document != null)
                    {
                        results.Add(new VKAIVectorRecord<T>(
                            reader.GetString(0),
                            document,
                            score
                        ));
                    }
                }
            }

            VKAIVectorStoreDiagnostics.RecordSearchDuration(sw.Elapsed.TotalSeconds);
            return VKResult.Success(results.AsEnumerable());
        }
        catch (SqliteException ex)
        {
            _logger.CommandFailed(ex, $"Search in {collectionName}");
            VKAIVectorStoreSqliteDiagnostics.RecordError();
            return VKResult.Failure<IEnumerable<VKAIVectorRecord<T>>>(Errors.Database.ExecutionFailed);
        }
    }

    internal async Task<VKResult> DeleteGenericAsync(string collectionName, string id, string tenantId, CancellationToken ct)
    {
        await EnsureCollectionInitializedAsync(collectionName, ct).ConfigureAwait(false);

        using var connection = await GetOpenConnectionAsync(ct).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();

        var sw = Stopwatch.StartNew();
        try
        {
            var metaTable = GetMetadataTableName(collectionName);
            var vecTable = GetVectorTableName(collectionName);

            string getSql = $"SELECT ROWID FROM {metaTable} WHERE Id = @id";
            using var getCmd = new SqliteCommand(getSql, connection, transaction);
            getCmd.Parameters.AddWithValue("@id", id);

            var rowId = (long?)await _pipeline.ExecuteAsync(async ct => await getCmd.ExecuteScalarAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            if (rowId != null)
            {
                string delMetaSql = $"DELETE FROM {metaTable} WHERE ROWID = @rowid";
                using var delMetaCmd = new SqliteCommand(delMetaSql, connection, transaction);
                delMetaCmd.Parameters.AddWithValue("@rowid", rowId);
                await _pipeline.ExecuteAsync(async ct => await delMetaCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

                string delVecSql = $"DELETE FROM {vecTable} WHERE rowid = @rowid";
                using var delVecCmd = new SqliteCommand(delVecSql, connection, transaction);
                delVecCmd.Parameters.AddWithValue("@rowid", rowId);
                await _pipeline.ExecuteAsync(async ct => await delVecCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);
            }

            transaction.Commit();
            VKAIVectorStoreSqliteDiagnostics.RecordDeleteDuration(sw.Elapsed.TotalSeconds);
        }
        catch (SqliteException ex)
        {
            transaction.Rollback();
            _logger.CommandFailed(ex, $"Delete from {collectionName}");
            VKAIVectorStoreSqliteDiagnostics.RecordError();
            return VKResult.Failure(Errors.Database.ExecutionFailed);
        }

        return VKResult.Success();
    }



    private string GetMetadataTableName(string collectionName) => $"VK_AI_Metadata_{collectionName}";
    private string GetVectorTableName(string collectionName) => $"VK_AI_Vectors_{collectionName}";

    private async Task<SqliteConnection> GetOpenConnectionAsync(CancellationToken ct)
    {
        var connection = new SqliteConnection(_options.Connection);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        try
        {
            connection.EnableExtensions(true);
            var extensionPath = GetExtensionPath();
            connection.LoadExtension(extensionPath);
        }
        catch (SqliteException ex)
        {
            _logger.ExtensionLoadFailed(ex);
        }

        return connection;
    }

    private static string GetExtensionPath()
    {
        var extensionFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "vec0.dll" :
                               RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "vec0.dylib" : "vec0.so";

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new NotSupportedException($"Architecture {RuntimeInformation.ProcessArchitecture} is not supported by sqlite-vec.")
        };

        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                       RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "linux";

        var rid = $"{platform}-{architecture}";

        var path = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", extensionFileName);
        if (File.Exists(path))
            return path;

        path = Path.Combine(AppContext.BaseDirectory, extensionFileName);
        if (File.Exists(path))
            return path;

        return extensionFileName;
    }

    private async Task EnsureCollectionInitializedAsync(string collectionName, CancellationToken ct)
    {
        if (_initializedCollections.Contains(collectionName))
            return;

        await _initLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_initializedCollections.Contains(collectionName))
                return;

            using var connection = await GetOpenConnectionAsync(ct).ConfigureAwait(false);

            var metaTable = GetMetadataTableName(collectionName);
            var vecTable = GetVectorTableName(collectionName);

            // 1. Create Metadata Table (Generic Schema)
            string metaSql = $@"
                CREATE TABLE IF NOT EXISTS {metaTable} (
                    Id TEXT PRIMARY KEY,
                    Data TEXT NOT NULL
                );";

            using var metaCmd = new SqliteCommand(metaSql, connection);
            await _pipeline.ExecuteAsync(async ct => await metaCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            // 2. Create Virtual Vector Table (vec0)
            string vecSql = @$"
                CREATE VIRTUAL TABLE IF NOT EXISTS {vecTable} USING vec0(
                    embedding float[{_options.EmbeddingDimension}]
                );";

            using var vecCmd = new SqliteCommand(vecSql, connection);
            await _pipeline.ExecuteAsync(async ct => await vecCmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false), ct).ConfigureAwait(false);

            _initializedCollections.Add(collectionName);
            VKAIVectorStoreSqliteDiagnostics.RecordCollectionInit();
            _logger.DatabaseInitialized($"{_options.Connection} [{collectionName}]");
        }
        catch (SqliteException ex)
        {
            _logger.CommandFailed(ex, $"Initialization of {collectionName}");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

}
