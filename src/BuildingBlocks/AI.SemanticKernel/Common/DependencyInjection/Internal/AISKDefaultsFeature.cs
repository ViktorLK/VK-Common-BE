using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;

/// <summary>
/// Custom registration logic hook for Semantic Kernel defaults feature.
/// </summary>
internal sealed partial class AISKDefaultsFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKAISKDefaultsOptions options)
    {
        services.TryAddSingleton<IVKAISKOptionsProvider, VKAISKDefaultOptionsProvider>();
    }
}
