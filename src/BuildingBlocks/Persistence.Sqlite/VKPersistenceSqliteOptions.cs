using VK.Blocks.Core;

namespace VK.Blocks.Persistence.Sqlite;

/// <summary>
/// Configuration options for the SQLite persistence layer.
/// </summary>
public sealed record VKPersistenceSqliteOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Persistence:Sqlite";

    /// <summary>
    /// Gets a value indicating whether the persistence block is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the database connection string for SQLite.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;
}
