using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Databases.Internal;

/// <summary>
/// Marker class for the In-Memory feature.
/// Following the sub-feature pattern from Authorization.
/// </summary>
[VKFeatureMarker("InMemory", typeof(VKAIVectorStoreBlock))]
internal sealed partial class InMemoryFeatureMarker;
