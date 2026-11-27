using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Synapse.Security.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

[VKFeature(typeof(VKAISynapseBlock), OptionsType = typeof(VKSecurityOptions))]
internal sealed partial class SecurityFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSecurityOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKConnectionValidator, DefaultConnectionValidator>();
        services.Replace(ServiceDescriptor.Singleton<IVKAICredentialResolver, ConfigurationAICredentialResolver>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKSecurityOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
