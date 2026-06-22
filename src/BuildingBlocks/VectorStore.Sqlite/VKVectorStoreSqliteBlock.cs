using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite;

/// <summary>
/// Architectural marker for the AI Vector Store SQLite extension.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKVectorStoreBlock)])]
public sealed partial class VKVectorStoreSqliteBlock;
