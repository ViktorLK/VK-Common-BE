namespace VK.Blocks.AI.VectorStore.Sqlite.Diagnostics;

/// <summary>
/// Constants for SQLite AI Vector Store diagnostics.
/// Follows OR.01 requirement for centralized semantic tokens.
/// </summary>
internal static class AIVectorStoreSqliteDiagnosticsConstants
{
    public static class Metrics
    {
        public const string DatabaseErrors = "vk_ai_vector_sqlite_errors";
        public const string CollectionsInitialized = "vk_ai_vector_sqlite_collections_total";
        public const string UpsertDuration = "vk_ai_vector_sqlite_upsert_duration";
        public const string DeleteDuration = "vk_ai_vector_sqlite_delete_duration";
    }
}
