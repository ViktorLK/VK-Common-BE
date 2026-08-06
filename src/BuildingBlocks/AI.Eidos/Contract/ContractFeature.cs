using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Contract.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKContractOptions))]
internal sealed partial class ContractFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKContractOptions options)
    {
        services.TryAddSingleton<IVKContractRegistry, DefaultContractRegistry>();
        services.TryAddSingleton<IVKContractResolver, DefaultContractResolver>();
        services.TryAddSingleton<IVKContractMigrator, DefaultContractMigrator>();
    }
}
