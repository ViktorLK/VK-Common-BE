using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Public diagnostic tokens for the SQLite AI Vector Store.
/// </summary>
public static class VKDiagnosticsConstants
{
    // Logs (Event IDs)
    public static class Logs
    {
        public const int DatabaseInitialized = VKDiagnosticOffsets.AI_Vectorics + 501;
        public const int CommandFailed = VKDiagnosticOffsets.AI_Vectorics + 502;
        public const int ExtensionLoadFailed = VKDiagnosticOffsets.AI_Vectorics + 503;
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string DatabaseErrors = "vk_ai_vector_sqlite_errors";
        public const string CollectionsInitialized = "vk_ai_vector_sqlite_collections_total";
        public const string UpsertDuration = "vk_ai_vector_sqlite_upsert_duration";
        public const string DeleteDuration = "vk_ai_vector_sqlite_delete_duration";
    }
}
