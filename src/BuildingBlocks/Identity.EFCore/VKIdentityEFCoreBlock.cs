using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Identity.EFCore.TenantUser.Internal;
using VK.Blocks.Persistence;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Identity.EFCore;

/// <summary>
/// Identity.EFCore Building Block Marker.
/// Provides EFCore-backed implementations for all Identity repositories and model creating contributors.
/// Follows BB.02, AP.01, AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKIdentityBlock), typeof(VKPersistenceEFCoreBlock)], Toggleable = false)]
public sealed partial class VKIdentityEFCoreBlock
{
    static partial void RegisterBlockCustom(IVKIdentityEFCoreBuilder builder)
    {
        var services = builder.Services;

        // 1. TenantUser Domain Repository (Composite Key Aggregate)
        services.TryAddScoped<IVKIdentityTenantUserRepository, IdentityTenantUserRepository>();

        // 2. Personal Data Permission Query Contributor (Scoped)
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKQueryContributor, VKPersonalDataPermissionQueryContributor>());
    }
}
