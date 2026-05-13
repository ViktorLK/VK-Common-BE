using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.VectorStore.Sqlite.Internal;

/// <summary>
/// Structured logging for the SQLite Vector Store.
/// </summary>
internal static partial class AIVectorStoreSqliteLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Initialized SQLite vector database at {Connection}")]
    public static partial void DatabaseInitialized(this ILogger logger, string connection);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to execute SQLite command: {Sql}")]
    public static partial void CommandFailed(this ILogger logger, Exception ex, string sql);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to load sqlite-vec extension. Falling back to potential built-in support or failing if missing.")]
    public static partial void ExtensionLoadFailed(this ILogger logger, Exception ex);
}
