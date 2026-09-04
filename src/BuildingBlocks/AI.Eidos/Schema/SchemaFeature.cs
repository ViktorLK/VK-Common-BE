using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Eidos.Schema.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

[VKFeature(typeof(VKAIEidosBlock), OptionsType = typeof(VKSchemaOptions))]
internal sealed partial class SchemaFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSchemaOptions options)
    {
        services.TryAddSingleton<IVKSchemaResolver, DefaultSchemaResolver>();
        services.TryAddSingleton<IVKSchemaMigrator, DefaultSchemaMigrator>();
        services.TryAddSingleton<IVKSchemaEvolutionAnalyzer, DefaultSchemaEvolutionAnalyzer>();

        // Register Before Stage in Psyche Pipeline
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPsychePipelineStage, DefaultSchemaStage>());
    }
}
