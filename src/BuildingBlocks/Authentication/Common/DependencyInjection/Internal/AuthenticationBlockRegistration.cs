using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Common.Internal;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.DependencyInjection.Internal;

/// <summary>
/// Handles the actual core DI registration for the Authentication block.
/// Manual partial registration implementing the OnRegister hook.
/// </summary>
// [SG Registration]
internal static partial class AuthenticationBlockRegistration
{
    // [SG Hook]
    static partial void RegisterBlockCustom(IVKAuthenticationBuilder builder)
    {
        var services = builder.Services;

        // 1. HttpContextAccessor is required by ClaimsTransformer
        services.AddHttpContextAccessor();

        // 2. Register core Claims Transformer BEFORE AddAuthentication so it wins over NoopClaimsTransformation
        services.TryAddTransient<IClaimsTransformation, ClaimsTransformer>();

        // 3. Automatically enable core defaults (Provider, Scheme, etc.)
        AuthenticationDefaultsFeature.Register(builder);
        var defaultsOptions = services.GetVKServiceInstance<VKAuthenticationDefaultsOptions>()!;

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
