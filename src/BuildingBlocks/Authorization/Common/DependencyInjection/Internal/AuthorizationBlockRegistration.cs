using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Common.Diagnostics.Internal;
using VK.Blocks.Authorization.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Common.DependencyInjection.Internal;

// [SG Registration]
internal static partial class AuthorizationBlockRegistration
{
    // [SG Hook]
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

        // 4. Automatically enable core defaults
        AuthorizationDefaultsFeature.Register(builder);
    }
}
