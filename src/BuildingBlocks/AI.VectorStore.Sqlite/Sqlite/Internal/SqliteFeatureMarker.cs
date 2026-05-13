using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Sqlite.Internal;

/// <summary>
/// Marker class for the SQLite vector store feature.
/// </summary>
[VKFeatureMarker("Sqlite", typeof(VKAIVectorStoreBlock))]
internal sealed partial class SqliteFeatureMarker;
