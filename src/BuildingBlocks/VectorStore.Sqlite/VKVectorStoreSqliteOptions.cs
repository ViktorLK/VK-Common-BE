using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Options for configuring the SQLite AI Vector Store extension.
/// </summary>
public sealed record VKVectorStoreSqliteOptions : IVKBlockOptions
{
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKVectorStoreSqliteBlock.BlockName}";

    /// <summary>
    /// Gets or sets a value indicating whether the SQLite Vector Store extension is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

}
