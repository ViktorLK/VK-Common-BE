using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Internal;

/// <summary>
/// Pipeline stage responsible for resolving AI model identity, provider, and physical token capabilities before prompt weaving.
/// Merges request-scoped <see cref="VKChatArgs"/> with ambient <see cref="VKChatOptions"/> and caches a guaranteed <see cref="VKAIModelMetadata"/>.
/// Follows AP.01 (sealed class default), CS.01, CS.03, CS.07, BB.04, and OR.01.
/// </summary>
[VKTrace("psyche.stage.model_resolve")]
internal sealed class DefaultModelResolveStage : IVKPsychePipelineStage
{
    private readonly IVKVKAIModelCatalog _modelCatalog;
    private readonly ILogger<DefaultModelResolveStage> _logger;

    public DefaultModelResolveStage(
        IVKVKAIModelCatalog modelCatalog,
        ILogger<DefaultModelResolveStage> logger)
    {
        _modelCatalog = VKGuard.NotNull(modelCatalog); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheModelResolve;

    public bool IsActive => true;
    public string TraceName => "psyche.stage.model_resolve";
    public string StageName => "ModelResolve";

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context); // [AP.01]
        cancellationToken.ThrowIfCancellationRequested(); // [CS.03]

        // 1. Resolve ambient chat options (fallback if not explicitly passed in request args)
        var chatOptions = context.Services.GetService<VKChatOptions>()
            ?? context.Services.GetService<IOptions<VKChatOptions>>()?.Value; // [CS.07] Documented fallback to optional ambient options

        // 2. Resolve Provider first, then ModelId
        var chatArgs = context.Args<VKChatArgs>();
        var configuredProvider = chatArgs?.Provider ?? chatOptions?.Provider;
        var modelId = chatArgs?.ModelId ?? chatOptions?.ModelId;

        // 3. Fail-fast if provider is unconfigured
        if (!configuredProvider.HasValue)
        {
            if (context.IsWeaveOnly)
            {
                _logger.ModelResolveSkipped();
                ResolveTokenBudget(context, null);
                return Task.FromResult(VKResult.Success()); // [CS.01]
            }

            return Task.FromResult(VKResult.Failure(VKPipelineErrors.ProviderNotConfigured)); // [CS.01]
        }

        // 4. Fail-fast if model is unconfigured
        if (string.IsNullOrWhiteSpace(modelId))
        {
            if (context.IsWeaveOnly)
            {
                _logger.ModelResolveSkipped();
                ResolveTokenBudget(context, null);
                return Task.FromResult(VKResult.Success()); // [CS.01]
            }

            return Task.FromResult(VKResult.Failure(VKPipelineErrors.ModelNotConfigured)); // [CS.01]
        }

        // 5. Resolve physical capability metadata from catalog with strictly non-null provider
        var metadata = _modelCatalog.GetAIModelMetadata(configuredProvider.Value, modelId);

        context.SetState(metadata);
        _logger.ModelResolved(modelId, metadata.Provider.ToString(), metadata.ContextWindowSize);

        // 6. Upfront resolution of global TokenBudget contract (Single Source of Truth)
        ResolveTokenBudget(context, metadata.ContextWindowSize);

        return Task.FromResult(VKResult.Success()); // [CS.01]
    }

    private static void ResolveTokenBudget(VKPsycheContext context, int? modelContextWindowSize)
    {
        var psycheOptions = context.Services.GetService<VKAIPsycheOptions>()
            ?? context.Services.GetService<IOptions<VKAIPsycheOptions>>()?.Value;
        var weavingOptions = context.Services.GetService<VKWeavingOptions>()
            ?? context.Services.GetService<IOptions<VKWeavingOptions>>()?.Value;
        var weavingArgs = context.Args<VKWeavingArgs>();

        var configuredTotal = weavingArgs?.MaxContextBudget
            ?? weavingOptions?.MaxContextBudget
            ?? psycheOptions?.MaxContextBudget;

        var totalLimit = configuredTotal ?? modelContextWindowSize;

        var reservedResponse = weavingArgs?.ResponseReservedTokens
            ?? weavingOptions?.ResponseReservedTokens
            ?? psycheOptions?.ResponseReservedTokens
            ?? 4096;

        context.TokenBudget = new VKPsycheTokenBudget
        {
            TotalLimit = totalLimit,
            ReservedResponseTokens = reservedResponse
        };
    }
}
