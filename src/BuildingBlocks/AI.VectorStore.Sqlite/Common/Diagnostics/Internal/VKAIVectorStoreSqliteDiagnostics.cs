using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite;

/// <summary>
/// Provides diagnostic metrics for the SQLite Vector Store extension.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Infrastructure class for telemetry instrumentation")]
[VKBlockDiagnostics<VKAIVectorStoreSqliteBlock>]
public static partial class VKAIVectorStoreSqliteDiagnostics
{
    private static readonly Counter<long> DatabaseErrors;
    private static readonly Counter<long> CollectionsInitialized;
    private static readonly Histogram<double> UpsertDuration;
    private static readonly Histogram<double> DeleteDuration;

    static VKAIVectorStoreSqliteDiagnostics()
    {
        DatabaseErrors = Meter!.CreateCounter<long>(
            VKAIVectorStoreSqliteDiagnosticsConstants.Metrics.DatabaseErrors,
            "errors",
            "Number of SQLite specific execution errors");

        CollectionsInitialized = Meter!.CreateCounter<long>(
            VKAIVectorStoreSqliteDiagnosticsConstants.Metrics.CollectionsInitialized,
            "collections",
            "Number of unique collections initialized in this session");

        UpsertDuration = Meter!.CreateHistogram<double>(
            VKAIVectorStoreSqliteDiagnosticsConstants.Metrics.UpsertDuration,
            "seconds",
            "Duration of SQLite upsert operations");

        DeleteDuration = Meter!.CreateHistogram<double>(
            VKAIVectorStoreSqliteDiagnosticsConstants.Metrics.DeleteDuration,
            "seconds",
            "Duration of SQLite delete operations");
    }

    public static void RecordError() => DatabaseErrors.Add(1);
    public static void RecordCollectionInit() => CollectionsInitialized.Add(1);
    public static void RecordUpsertDuration(double seconds) => UpsertDuration.Record(seconds);
    public static void RecordDeleteDuration(double seconds) => DeleteDuration.Record(seconds);
}
