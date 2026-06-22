using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite.Common.Diagnostics.Internal;

/// <summary>
/// Provides unified diagnostics and structured logging for the SQLite Vector Store extension.
/// Following the integrated diagnostics pattern.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Infrastructure class for telemetry instrumentation")]
[VKBlockDiagnostics<VKVectorStoreSqliteBlock>]
internal static partial class AIVectorStoreSqliteDiagnostics
{
    // --- 1. [LoggerMessage] Log Message Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.DatabaseInitialized,
        Level = LogLevel.Information,
        Message = "Initialized SQLite vector database at {Connection}")]
    public static partial void DatabaseInitialized(this ILogger logger, string connection);

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.CommandFailed,
        Level = LogLevel.Error,
        Message = "Failed to execute SQLite command: {Sql}")]
    public static partial void CommandFailed(this ILogger logger, Exception ex, string sql);

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.ExtensionLoadFailed,
        Level = LogLevel.Warning,
        Message = "Failed to load sqlite-vec extension. Falling back to potential built-in support or failing if missing.")]
    public static partial void ExtensionLoadFailed(this ILogger logger, Exception ex);

    // --- 2. Telemetry Metrics ---

    private static readonly Counter<long> DatabaseErrors;
    private static readonly Counter<long> CollectionsInitialized;
    private static readonly Histogram<double> UpsertDuration;
    private static readonly Histogram<double> DeleteDuration;

    static AIVectorStoreSqliteDiagnostics()
    {
        DatabaseErrors = Meter!.CreateCounter<long>(
            VKDiagnosticsConstants.Metrics.DatabaseErrors,
            "errors",
            "Number of SQLite specific execution errors");

        CollectionsInitialized = Meter!.CreateCounter<long>(
            VKDiagnosticsConstants.Metrics.CollectionsInitialized,
            "collections",
            "Number of unique collections initialized in this session");

        UpsertDuration = Meter!.CreateHistogram<double>(
            VKDiagnosticsConstants.Metrics.UpsertDuration,
            "seconds",
            "Duration of SQLite upsert operations");

        DeleteDuration = Meter!.CreateHistogram<double>(
            VKDiagnosticsConstants.Metrics.DeleteDuration,
            "seconds",
            "Duration of SQLite delete operations");
    }

    public static void RecordError() => DatabaseErrors.Add(1);
    public static void RecordCollectionInit() => CollectionsInitialized.Add(1);
    public static void RecordUpsertDuration(double seconds) => UpsertDuration.Record(seconds);
    public static void RecordDeleteDuration(double seconds) => DeleteDuration.Record(seconds);
}
