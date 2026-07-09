namespace VK.Blocks.Persistence.Common.Diagnostics;

/// <summary>
/// Constants for persistence diagnostics.
/// </summary>
internal static class DiagnosticsConstants
{
    /// <summary>Source name for persistence activities.</summary>
    public const string SourceName = "VK.Blocks.Persistence";

    // --- Activity Names ---
    public const string ActivitySaveChanges = "Persistence.SaveChanges";
    public const string ActivityBeginTransaction = "Persistence.BeginTransaction";
    public const string ActivityCommitTransaction = "Persistence.CommitTransaction";
    public const string ActivityQuery = "Persistence.Query";
    public const string ActivityBulkOperation = "Persistence.BulkOperation";

    // --- Meter Names ---
    public const string MeterName = "VK.Blocks.Persistence";

    // --- Metric Names ---
    public const string MetricSaveChangesDuration = "persistence.save_changes.duration";
    public const string MetricSaveChangesCount = "persistence.save_changes.count";
    public const string MetricQueryDuration = "persistence.query.duration";
    public const string MetricQueryCount = "persistence.query.count";
    public const string MetricTransactionDuration = "persistence.transaction.duration";
    public const string MetricBulkRowsAffected = "persistence.bulk.rows_affected";
    public const string MetricConcurrencyRetries = "persistence.concurrency.retries";
}
