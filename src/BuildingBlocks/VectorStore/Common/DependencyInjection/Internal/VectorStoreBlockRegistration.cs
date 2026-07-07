using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorStore.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class VectorStoreBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKVectorStoreBuilder builder)
    {
        // Register defaults feature
        VectorStoreDefaultsFeature.Register(builder);
    }
}
