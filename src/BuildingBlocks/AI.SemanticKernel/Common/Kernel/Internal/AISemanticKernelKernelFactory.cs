using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.PromptTemplates.Liquid;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Industrial implementation of <see cref="IAISemanticKernelKernelFactory"/>.
/// Handles the complex weaving of AI connectors and plugins.
/// </summary>
internal sealed class AISemanticKernelKernelFactory(
    IVKAISemanticKernelOptionsProvider optionsProvider,
    IOptions<VKAIOptions> globalOptions,
    IVKChatOptionsProvider chatOptions,
    IOptions<VKEmbeddingsOptions> embeddingOptions,
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider) : IAISemanticKernelKernelFactory
{
    /// <inheritdoc />
    public Microsoft.SemanticKernel.Kernel CreateKernel()
    {
        var options = optionsProvider.GetOptions();
        var globalAiOptions = globalOptions.Value;
        var chatFeatureOptions = chatOptions.GetOptions();
        var embeddingFeatureOptions = embeddingOptions.Value;
        
        // Dynamically resolve client by VKAIOptions.HttpClientName (with fallback)
        var clientName = !string.IsNullOrWhiteSpace(globalAiOptions.HttpClientName)
            ? globalAiOptions.HttpClientName
            : AISemanticKernelConstants.HttpClientName;
        var httpClient = httpClientFactory.CreateClient(clientName);

        IKernelBuilder builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

        // 1. Register Template Factory (Industrial DNA: Specialized Engines)
        switch (options.TemplateFormat)
        {
            case AISemanticKernelTemplateFormat.Handlebars:
                builder.Services.AddSingleton<IPromptTemplateFactory, HandlebarsPromptTemplateFactory>();
                break;
            case AISemanticKernelTemplateFormat.Liquid:
                builder.Services.AddSingleton<IPromptTemplateFactory, LiquidPromptTemplateFactory>();
                break;
        }

        // 2. Register AI Services (Multi-Provider Support)
        if (chatFeatureOptions.Enabled)
        {
            builder.RegisterChatService(options, chatFeatureOptions, httpClient, serviceId: "primary");
        }

        if (embeddingFeatureOptions.Enabled)
        {
            builder.RegisterEmbeddingService(options, embeddingFeatureOptions, httpClient);
        }

        // 3. Register Dynamic Plugins from DI container
        var pluginProviders = serviceProvider.GetServices<IAISemanticKernelPluginProvider>();
        foreach (var provider in pluginProviders)
        {
            provider.Register(builder, serviceProvider);
        }

        // 4. Register Industrial DNA Filters (Guardrails)
        var promptFilters = serviceProvider.GetServices<IPromptRenderFilter>();
        foreach (var filter in promptFilters)
        {
            builder.Services.AddSingleton(filter);
        }

        var functionFilters = serviceProvider.GetServices<IFunctionInvocationFilter>();
        foreach (var filter in functionFilters)
        {
            builder.Services.AddSingleton(filter);
        }

        // [Phase 2] Register IAutoFunctionInvocationFilter for automatic tool-call guardrails
        var autoFunctionFilters = serviceProvider.GetServices<IAutoFunctionInvocationFilter>();
        foreach (var filter in autoFunctionFilters)
        {
            builder.Services.AddSingleton(filter);
        }

        return builder.Build();
    }
}
