using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Authentication.Common.Protocols;
using VK.Blocks.Authentication.Generated;

namespace VK.Blocks.Authentication;

/// <summary>
/// A marker type for the VK.Blocks.Authentication building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKAuthenticationBlock
{

    static partial void RegisterBlockCustom(IVKAuthenticationBuilder builder)
    {
        var services = builder.Services;

        // 1. HttpContextAccessor is required by ClaimsTransformer
        services.AddHttpContextAccessor();

        // 2. Register core Claims Transformer BEFORE AddAuthentication so it wins over NoopClaimsTransformation
        services.TryAddTransient<IClaimsTransformation, ClaimsTransformer>();

        // Register Global Single Sign-Out coordinator
        services.TryAddSingleton<IVKSloHandler, DefaultSloHandler>();

        var defaultsOptions = services.GetVKServiceInstance<VKAuthenticationOptions>()!;

        // 4. Framework Integration
        AuthenticationBuilder authBuilder = services.AddAuthentication(authOpts =>
        {
            authOpts.DefaultScheme = defaultsOptions.DefaultScheme;
            authOpts.DefaultAuthenticateScheme = defaultsOptions.DefaultScheme;
            authOpts.DefaultChallengeScheme = defaultsOptions.DefaultScheme;
        });

        // 5. Auto-Discovery
        services.AddGeneratedClaimsProviders();

        // 6. Set the captured AuthenticationBuilder on our generated builder
        builder.AuthBuilder = authBuilder;
    }

}
