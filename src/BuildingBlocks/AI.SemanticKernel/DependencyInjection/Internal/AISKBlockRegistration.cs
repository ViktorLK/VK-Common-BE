using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.Chat.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Embeddings.Internal;
using VK.Blocks.AI.SemanticKernel.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Retrieval.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Semantic Kernel building block.
/// </summary>
internal static class AISKBlockRegistration
{
    public static IVKAISKBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAISKOptions, VKAISKOptions>? configure = null)
    {
        VKGuard.NotNull(services);

        IVKAISKBuilder builder = new AISKBlockBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKAISKBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAISKOptions options = services.AddVKBlockOptions(configuration, configure);

        // Register core and feature sub-options
        services.AddVKBlockOptions<VKAIOptions>(configuration);
        services.AddVKBlockOptions<VKChatOptions>(configuration);
        services.AddVKBlockOptions<VKEmbeddingOptions>(configuration);
        services.AddVKBlockOptions<VKRetrievalOptions>(configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKAISKBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAISKOptions>, AISKOptionsValidator>());

        // 5. Diagnostics handled by Source Generator

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Diagnostics Filters
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IFunctionInvocationFilter, AISKDiagnosticsFilter>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IPromptRenderFilter, AISKDiagnosticsFilter>());

        // 8. Dynamic Options Provider
        services.TryAddScoped<DefaultAISKOptionsProvider>();
        services.TryAddScoped<IAISKOptionsProvider, DefaultAISKOptionsProvider>();
        services.TryAddScoped<IAISKKernelFactory, AISKKernelFactory>();

        // 9. Core Services
        RegisterCoreServices(services);

        return builder;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        // 1. HttpClient with Resilience
        services.AddHttpClient(AISKConstants.HttpClientName)
            .AddStandardResilienceHandler();

        // 2. Kernel Registration (Delegated to Factory)
        services.TryAddScoped(sp => sp.GetRequiredService<IAISKKernelFactory>().CreateKernel());

        // 3. Feature Registrations (Generic Version)
        services.TryAddScoped<IVKChatEngine, AISKChatEngine>();
        services.TryAddScoped<IVKEmbeddingEngine, AISKEmbeddingEngine>();
        services.TryAddScoped<AISKRetrievalEngine>();
    }
}
