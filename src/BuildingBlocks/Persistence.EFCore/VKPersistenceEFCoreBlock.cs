using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.EFCore building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKPersistenceEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKPersistenceEFCoreBuilder builder)
    {
        var options = builder.Services.GetVKServiceInstance<VKPersistenceEFCoreOptions>();
        if (options is null) return;

        if (options.EnableAuditing == true)
        {
            builder.Services.TryAddScoped<VKAuditingInterceptor>();
        }

        if (options.EnableSoftDelete == true)
        {
            builder.Services.TryAddScoped<VKSoftDeleteInterceptor>();
        }

        if (options.EnableMultiTenancy == true)
        {
            builder.Services.TryAddScoped<VKTenantInterceptor>();
        }
    }
}
