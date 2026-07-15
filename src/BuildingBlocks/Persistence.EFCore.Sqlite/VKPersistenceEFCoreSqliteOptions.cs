using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Sqlite;

/// <summary>
/// Configuration options for the SQLite persistence layer.
/// </summary>
public sealed partial record VKPersistenceEFCoreSqliteOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the database connection string for SQLite.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;
}
