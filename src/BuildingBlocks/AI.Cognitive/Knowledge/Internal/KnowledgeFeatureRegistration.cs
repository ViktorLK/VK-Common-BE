using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive.DependencyInjection.Internal;
using VK.Blocks.AI.Cognitive.Knowledge.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Knowledge.Internal;

internal static class KnowledgeFeatureRegistration
{
    public static IVKAICognitiveBuilder Register(IVKAICognitiveBuilder builder)
    {
        VKGuard.NotNull(builder);

        if (builder.Services.IsVKServiceRegistered<KnowledgeFeature>())
        {
            return builder;
        }

        builder.Services.AddSingleton<KnowledgeFeature>();

        VKKnowledgeOptions options = builder.Services.AddVKBlockOptions<VKKnowledgeOptions>(builder.Configuration);

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKKnowledgeOptions>, KnowledgeOptionsValidator>());

        if (!options.Enabled)
        {
            return builder;
        }

        builder.Services.TryAddScoped<IVKKnowledgeManager, VKKnowledgeManager>();

        return builder;
    }
}
