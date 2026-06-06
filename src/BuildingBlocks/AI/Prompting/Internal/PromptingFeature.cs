using VK.Blocks.AI.Prompting.Diagnostics.Internal;
using VK.Blocks.AI.Prompting.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Prompting.Internal;

/// <summary>
/// Prompting feature marker and registration hub.
/// </summary>
internal sealed partial class PromptingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPromptingOptions options)
    {
        services.TryAddSingleton<IVKPromptTemplateEngine, BasicVKPromptTemplateEngine>();
        services.TryAddSingleton<IVKPromptRegistry, VKPromptRegistry>();

        if (!string.IsNullOrWhiteSpace(options.BaseDirectory))
        {
            services.AddSingleton<IVKPromptProvider>(new FileVKPromptProvider(options.BaseDirectory));
        }

        if (!string.IsNullOrWhiteSpace(options.EmbeddedPromptNamespace))
        {
            var assembly = options.EmbeddedPromptAssembly ?? typeof(PromptingFeature).Assembly;
            services.AddSingleton<IVKPromptProvider>(new EmbeddedVKPromptProvider(assembly, options.EmbeddedPromptNamespace));
        }
    }

    /// <summary>Add prompting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKPromptingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
