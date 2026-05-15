using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.VectorStore.Internal;

/// <summary>
/// Domain-specific errors for the SQLite Vector Store extension.
/// Following CS.01 error naming hierarchy.
/// </summary>
internal static class Errors
{
    public static class Database
    {
        public static readonly VKError ExecutionFailed = VKError.Failure(
            "AI.VectorStore.Sqlite.Database.ExecutionFailed",
            "An error occurred while executing a SQLite command.");
    }
}
