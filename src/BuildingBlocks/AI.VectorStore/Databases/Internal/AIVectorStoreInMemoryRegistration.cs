using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Databases.Internal;

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
        // 1. Check-Self (Idempotency)
        if (builder.Services.IsVKBlockRegistered<InMemoryFeatureMarker>())
        {
            return builder;
        }

        // 2. Mark-Self
        builder.Services.AddVKBlockMarker<InMemoryFeatureMarker>();

        // 3. Register Implementation
        builder.Services.AddScoped<IVKAIVectorDatabase, AIVectorStoreInMemoryDatabase>();

        return builder;
    }
}
