using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

namespace VK.Blocks.AI.SemanticKernel.Common.Routing.Internal;

internal sealed class VKAIProbeBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVKAIProviderTracker _tracker;
    private readonly ILogger<VKAIProbeBackgroundService> _logger;

    public VKAIProbeBackgroundService(
        IServiceProvider serviceProvider,
        IVKAIProviderTracker tracker,
        ILogger<VKAIProbeBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _tracker = tracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await ProbeAllAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during AI provider health probing.");
            }
        }
    }

    private async Task ProbeAllAsync(CancellationToken cancellationToken)
    {
        var providersInCooldown = _tracker.GetProvidersOnCooldown();
        if (providersInCooldown.Count == 0)
            return;

        _logger.LogInformation("Found {Count} AI providers on cooldown. Initiating health probes.", providersInCooldown.Count);

        using var scope = _serviceProvider.CreateScope();
        var AISemanticKernelOptions = scope.ServiceProvider.GetRequiredService<IOptions<VKAISemanticKernelOptions>>().Value;
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(AISemanticKernelConstants.HttpClientName);

        foreach (var providerOptions in providersInCooldown)
        {
            try
            {
                var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();
                builder.RegisterChatService(AISemanticKernelOptions, providerOptions, httpClient, "probe");
                var kernel = builder.Build();
                var chatService = kernel.GetRequiredService<IChatCompletionService>("probe");

                var history = new ChatHistory();
                history.AddUserMessage("Ping");

                var settings = new PromptExecutionSettings { ExtensionData = new Dictionary<string, object> { ["max_tokens"] = 1 } };
                var result = await chatService.GetChatMessageContentsAsync(history, settings, kernel, cancellationToken).ConfigureAwait(false);

                if (result != null && result.Count > 0)
                {
                    _logger.LogInformation("AI provider health probe succeeded for Model {ModelId}. Removing from cooldown.", providerOptions.ModelId);
                    _tracker.MarkSuccess(providerOptions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("AI provider health probe failed for Model {ModelId}. Exception: {Message}", providerOptions.ModelId, ex.Message);
            }
        }
    }
}
