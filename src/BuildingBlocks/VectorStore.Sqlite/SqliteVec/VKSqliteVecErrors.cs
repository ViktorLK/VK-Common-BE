using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Domain-specific errors for the SQLite Vector Store extension.
/// Following CS.01 error naming hierarchy.
/// </summary>
public static class VKSqliteVecErrors
{
    public static class Database
    {
        public static readonly VKError ExecutionFailed = VKError.Failure(
            "VectorStore.Sqlite.Database.ExecutionFailed",
            "An error occurred while executing a SQLite command.");
    }
}
