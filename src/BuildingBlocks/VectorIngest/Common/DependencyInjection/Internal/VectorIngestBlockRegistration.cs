using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorIngest.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class VectorIngestBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKVectorIngestBuilder builder)
    {
        // Custom registration logic goes here.
    }
}
