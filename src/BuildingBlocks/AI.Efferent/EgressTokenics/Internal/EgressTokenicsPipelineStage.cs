using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressTokenics.Internal;

internal sealed class EgressTokenicsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKEgressTokenicsOptions _options;
    private readonly ILogger<EgressTokenicsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(600, false, null, VKPipelinePhase.After);

    public EgressTokenicsPipelineStage(
        IVKTokenCounter tokenCounter,
        IOptionsSnapshot<VKEgressTokenicsOptions> options,
        ILogger<EgressTokenicsPipelineStage> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Response.ChatResponse?.Message is null)
        {
            return Task.FromResult(VKResult.Success());
        }

        var content = context.Response.ChatResponse.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            return Task.FromResult(VKResult.Success());
        }

        try
        {
            int tokenCount = _tokenCounter.CountTokens(content);
            _logger.LogInformation("Counted {Count} output tokens for generation response.", tokenCount);
            context.Response.TotalEstimatedTokens += tokenCount;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to count output tokens for generation response.");
        }

        return Task.FromResult(VKResult.Success());
    }
}
