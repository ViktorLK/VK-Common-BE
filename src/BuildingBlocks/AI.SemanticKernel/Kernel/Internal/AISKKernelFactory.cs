using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using VK.Blocks.AI.SemanticKernel.DependencyInjection.Internal;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Industrial implementation of <see cref="IVKAISKKernelFactory"/>.
/// Handles the complex weaving of AI connectors and plugins.
/// </summary>
internal sealed class AISKKernelFactory(
    IAISKOptionsProvider optionsProvider,
    IOptions<VKAIOptions> globalOptions,
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider) : IAISKKernelFactory
{
    /// <inheritdoc />
    public Microsoft.SemanticKernel.Kernel CreateKernel()
    {
        var options = optionsProvider.GetOptions();
        var globalAiOptions = globalOptions.Value;
        var httpClient = httpClientFactory.CreateClient(AISKConstants.HttpClientName);

        IKernelBuilder builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        // Since VKAISKOptions no longer provides ModelId, we look for global AI options.
        // In a real multi-feature scenario, the Kernel might be built per-feature or with multiple services.
        // For the default kernel, we use the global provider and its typical model naming convention.
        string modelId = globalAiOptions.Provider switch
        {
            VKAIProviderType.OpenAI => VKAIModelIds.OpenAI.Gpt4O,
            VKAIProviderType.AzureOpenAI => VKAIModelIds.OpenAI.Gpt4O,
            VKAIProviderType.Google => VKAIModelIds.Google.Gemini15Flash,
            _ => VKAIModelIds.OpenAI.Gpt4O
        };

        // 1. Register AI Services based on ServiceType
        if (options.ServiceType.Equals(nameof(VKAIProviderType.AzureOpenAI), StringComparison.OrdinalIgnoreCase))
        {
            builder.RegisterAzureOpenAI(options, modelId, isChat: true, httpClient);
            builder.RegisterAzureOpenAI(options, modelId, isChat: false, httpClient);
        }
        else if (options.ServiceType.Equals(nameof(VKAIProviderType.Google), StringComparison.OrdinalIgnoreCase))
        {
            builder.RegisterGoogleAI(options, modelId, isChat: true, httpClient);
            builder.RegisterGoogleAI(options, VKAIModelIds.Google.Embedding.TextEmbedding004, isChat: false, httpClient);
        }
        else if (options.ServiceType.Equals(nameof(VKAIProviderType.Ollama), StringComparison.OrdinalIgnoreCase))
        {
            builder.RegisterOllama(options, modelId);
        }
        else
        {
            // Default to OpenAI
            builder.RegisterOpenAI(options, modelId, httpClient);
        }

        // 2. Register Dynamic Plugins from DI container
        var pluginProviders = serviceProvider.GetServices<IAISKPluginProvider>();
        foreach (var provider in pluginProviders)
        {
            provider.Register(builder, serviceProvider);
        }

        return builder.Build();
    }
}
