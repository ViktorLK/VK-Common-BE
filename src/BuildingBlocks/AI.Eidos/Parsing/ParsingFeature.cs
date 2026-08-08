using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Parsing.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKParsingOptions))]
internal sealed partial class ParsingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKParsingOptions options)
    {
        services.TryAddSingleton<IVKContractValidator, DefaultContractValidator>();
        services.TryAddSingleton<IVKContractBinder, DefaultContractBinder>();
        services.TryAddSingleton<IVKContractStreamParser, DefaultContractStreamParser>();
        services.TryAddSingleton<IVKContractRepairService, DefaultContractRepairService>();
        services.TryAddSingleton<IVKContractExtractor, DefaultContractExtractor>();
    }
}

