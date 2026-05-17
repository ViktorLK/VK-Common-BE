using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Guardrails.Privacy.Internal;

/// <summary>
/// Privacy Guard feature marker and registration hub.
/// </summary>
internal sealed partial class PrivacyFeature
{
    /// <summary>Add privacy services here</summary>
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPrivacyOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKPrivacyFilter, NoOpVKPrivacyFilter>();
    }
}
