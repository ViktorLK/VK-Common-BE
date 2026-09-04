using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Identity.Context.Internal;
using VK.Blocks.Identity.User.Internal;

namespace VK.Blocks.Identity;

/// <summary>
/// Identity Building Block marker.
/// Follows [BB.02].
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Marker type used for dependency resolution and metadata; contains no business logic.")]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)], Toggleable = false)]
public sealed partial class VKIdentityBlock
{
    static partial void RegisterBlockCustom(IVKIdentityBuilder builder)
    {
        var services = builder.Services;

        // User slice services
        services.TryAddScoped<IVKUserClaimsPrincipalFactory, DefaultUserClaimsPrincipalFactory>();

        // Domain Model Factory (CS.06)
        services.TryAddSingleton<IVKIdentityModelFactory, Common.Internal.DefaultIdentityModelFactory>();

        // Dynamic SSoT User Context Accessor
        services.TryAddScoped<IVKUserContext, UserContextAccessor>();
    }
}
