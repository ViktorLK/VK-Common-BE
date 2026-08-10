using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Echo.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Echo feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKEchoOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class EchoFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEchoOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKEchoStore, InMemoryEchoStore>();

        switch (options.RenderStyle)
        {
            case VKEchoRenderStyle.Raw:
                services.TryAddSingleton<IVKEchoRenderer, RawEchoRenderer>();
                break;
            case VKEchoRenderStyle.Xml:
                services.TryAddSingleton<IVKEchoRenderer, XmlEchoRenderer>();
                break;
            case VKEchoRenderStyle.ChatML:
                services.TryAddSingleton<IVKEchoRenderer, ChatMLEchoRenderer>();
                break;
            case VKEchoRenderStyle.Header:
                services.TryAddSingleton<IVKEchoRenderer, HeaderEchoRenderer>();
                break;
            default:
            case VKEchoRenderStyle.Bracket:
                services.TryAddSingleton<IVKEchoRenderer, BracketEchoRenderer>();
                break;
        }

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultEchoExtractStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultEchoSaveStage>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultEchoFormatter>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKEchoOptions options, System.Collections.Generic.List<string> failures)
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
