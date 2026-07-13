using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.Persistence.EFCore.Common.DependencyInjection.Internal;

// [SG Registration]
internal sealed partial class PersistenceEFCoreBlock
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreBuilder builder)
    {
        PersistenceEFCoreBlock.Register(builder);
    }
}




