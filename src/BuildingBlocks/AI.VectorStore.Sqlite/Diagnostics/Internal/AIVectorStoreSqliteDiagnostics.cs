using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.Diagnostics.Internal;

/// <summary>
/// Provides diagnostic metrics for the SQLite Vector Store extension.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Infrastructure class for telemetry instrumentation")]
[VKBlockDiagnostics<VKAIVectorStoreSqliteBlock>]
internal static partial class AIVectorStoreSqliteDiagnostics
{
    private static readonly Counter<long> DatabaseErrors;
    private static readonly Counter<long> CollectionsInitialized;
    private static readonly Histogram<double> UpsertDuration;
    private static readonly Histogram<double> DeleteDuration;

    static AIVectorStoreSqliteDiagnostics()
    {
        DatabaseErrors = Meter!.CreateCounter<long>(
            AIVectorStoreSqliteDiagnosticsConstants.Metrics.DatabaseErrors,
            "errors",
            "Number of SQLite specific execution errors");

        CollectionsInitialized = Meter!.CreateCounter<long>(
            AIVectorStoreSqliteDiagnosticsConstants.Metrics.CollectionsInitialized,
            "collections",
            "Number of unique collections initialized in this session");

        UpsertDuration = Meter!.CreateHistogram<double>(
            AIVectorStoreSqliteDiagnosticsConstants.Metrics.UpsertDuration,
            "seconds",
            "Duration of SQLite upsert operations");

        DeleteDuration = Meter!.CreateHistogram<double>(
            AIVectorStoreSqliteDiagnosticsConstants.Metrics.DeleteDuration,
            "seconds",
            "Duration of SQLite delete operations");
    }

    public static void RecordError() => DatabaseErrors.Add(1);
    public static void RecordCollectionInit() => CollectionsInitialized.Add(1);
    public static void RecordUpsertDuration(double seconds) => UpsertDuration.Record(seconds);
    public static void RecordDeleteDuration(double seconds) => DeleteDuration.Record(seconds);
}
