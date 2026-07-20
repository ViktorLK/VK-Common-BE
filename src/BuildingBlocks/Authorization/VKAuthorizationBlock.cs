using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Common.Diagnostics.Internal;
using VK.Blocks.Authorization.Common.DependencyInjection.Internal;
using VK.Blocks.Authorization.Generated;

namespace VK.Blocks.Authorization;

/// <summary>
/// A marker type for the VK.Blocks.Authorization building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthorizationBlock
{

    static partial void RegisterBlockCustom(IVKAuthorizationBuilder builder)
    {
        var services = builder.Services;

        // 1. Diagnostics & Metadata
        services.TryAddEnumerableSingleton<IVKSecurityMetadataProvider, AuthorizationMetadataProvider>();

        // 2 ASP.NET Core base services
        services.AddAuthorization();
        services.TryAddEnumerableSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationPolicyProvider>();

        // 3. Register custom handlers discovered by Source Generator
        services.AddGeneratedAuthorizationHandlers();

    }

}
