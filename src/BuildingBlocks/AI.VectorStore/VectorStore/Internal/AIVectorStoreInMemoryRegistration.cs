using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.VectorStore.Internal;

/// <summary>
/// Provides internal registration logic for the In-Memory database feature.
/// </summary>
internal static class AIVectorStoreInMemoryRegistration
{
    /// <summary>
    /// Registers the In-Memory database provider.
    /// </summary>
    internal static IVKAIVectorStoreBuilder Register(IVKAIVectorStoreBuilder builder)
    {
        // 1. [BB.03] Strategy Check (Root-Driven Switching)
        var options = builder.Services.GetVKServiceInstance<VKAIVectorStoreOptions>();
        if (options?.Type != VKAIVectorStoreType.InMemory)
        {
            return builder;
        }

        // 2. Check-Self (Idempotency)
        if (builder.Services.IsVKBlockRegistered<InMemoryFeatureMarker>())
        {
            return builder;
        }

        // 3. Mark-Self
        builder.Services.AddVKBlockMarker<InMemoryFeatureMarker>();

        // 3. Register Implementation (Idempotent)
        builder.WithScoped<VKAIVectorStoreBlock, IVKAIVectorStore, AIVectorStoreInMemoryDatabase>();

        return builder;
    }
}
