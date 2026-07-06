using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorSearch.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class VectorSearchBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKVectorSearchBuilder builder)
    {
        // Custom registration logic goes here.
    }
}
