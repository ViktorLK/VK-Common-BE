using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite;

/// <summary>
/// Architectural marker for the AI Vector Store SQLite extension.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKAIVectorStoreBlock)])]
public sealed partial class VKAIVectorStoreSqliteBlock;
