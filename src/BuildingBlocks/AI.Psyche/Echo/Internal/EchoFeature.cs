using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Pipeline;

namespace VK.Blocks.AI.Psyche.Echo.Internal;

/// <summary>
/// Echo feature marker and registration hub.
/// </summary>
internal sealed partial class EchoFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKEchoOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKEchoStore, InMemoryEchoStore>();
        services.TryAddSingleton<IVKEchoRenderer, DefaultEchoRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingStage, DefaultEchoStage>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultEchoFormatter>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKEchoOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        if (options.TokenBudgetRatio < 0.0 || options.TokenBudgetRatio > 1.0)
        {
            failures.Add("TokenBudgetRatio must be a ratio value between 0.0 and 1.0 inclusive.");
        }

        if (options.MaxWindowSize.HasValue && options.MaxWindowSize.Value <= 0)
        {
            failures.Add("MaxWindowSize, if set, must be greater than zero.");
        }

        if (options.MaxTokens.HasValue && options.MaxTokens.Value <= 0)
        {
            failures.Add("MaxTokens, if set, must be greater than zero.");
        }

        if (options.MaxTurns.HasValue && options.MaxTurns.Value <= 0)
        {
            failures.Add("MaxTurns, if set, must be greater than zero.");
        }
    }
}
