namespace VK.Blocks.AI.VectorStore.Sqlite;

/// <summary>
/// Constants for SQLite AI Vector Store diagnostics.
/// Follows OR.01 requirement for centralized semantic tokens.
/// </summary>
public static class VKAIVectorStoreSqliteDiagnosticsConstants
{
    /// <summary>
    /// Telemetry metrics names.
    /// </summary>
    public static class Metrics
    {
        /// <summary>
        /// Total count of SQLite database errors.
        /// </summary>
        public const string DatabaseErrors = "vk_ai_vector_sqlite_errors";

        /// <summary>
        /// Total number of unique collections initialized.
        /// </summary>
        public const string CollectionsInitialized = "vk_ai_vector_sqlite_collections_total";

        /// <summary>
        /// Duration of sqlite-vec upsert operations.
        /// </summary>
        public const string UpsertDuration = "vk_ai_vector_sqlite_upsert_duration";

        /// <summary>
        /// Duration of sqlite-vec delete operations.
        /// </summary>
        public const string DeleteDuration = "vk_ai_vector_sqlite_delete_duration";
    }
}
