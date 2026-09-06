using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Materialization.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKMaterializationOptions))]
internal sealed partial class MaterializationFeature // [AP.01]
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKMaterializationOptions options)
    {
        services.TryAddSingleton<IVKMaterializationValidator, DefaultMaterializationValidator>();
        services.TryAddSingleton<IVKMaterializationBinder, DefaultMaterializationBinder>();
        services.TryAddSingleton<IVKMaterializationRepairService, DefaultMaterializationRepairService>();
        services.TryAddSingleton<IVKMaterializationRetryPolicy, DefaultMaterializationRetryPolicy>();

        // Register Onion Middleware in Psyche Pipeline
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPsycheMiddleware, DefaultMaterializationMiddleware>());
    }
}
