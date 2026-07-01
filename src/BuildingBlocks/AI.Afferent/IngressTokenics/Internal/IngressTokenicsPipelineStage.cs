using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressTokenics.Internal;

internal sealed class IngressTokenicsPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKIngressTokenicsOptions _options;
    private readonly ILogger<IngressTokenicsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new(600, false);

    public IngressTokenicsPipelineStage(
        IVKTokenCounter tokenCounter,
        IOptionsSnapshot<VKIngressTokenicsOptions> options,
        ILogger<IngressTokenicsPipelineStage> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var input = context.State<string>() ?? context.Request.UserInput;
        if (string.IsNullOrWhiteSpace(input))
        {
            return Task.FromResult(VKResult.Success());
        }

        int tokenCount;
        try
        {
            tokenCount = _tokenCounter.CountTokens(input);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count tokens for UserInput");
            return Task.FromResult(VKResult.Failure(IngressTokenicsErrors.CountingFailed));
        }

        if (tokenCount > _options.MaxInputTokens)
        {
            if (_options.EnforceHardLimit)
            {
                _logger.LogWarning("User input token count {Count} exceeds budget limit {Limit}.", tokenCount, _options.MaxInputTokens);
                return Task.FromResult(VKResult.Failure(IngressTokenicsErrors.BudgetExceeded));
            }
        }

        float usageRatio = (float)tokenCount / _options.MaxInputTokens;
        if (usageRatio >= _options.BudgetWarningThreshold)
        {
            _logger.LogWarning("User input token count {Count} is close to budget limit {Limit} (Usage: {Ratio:P}).", tokenCount, _options.MaxInputTokens, usageRatio);
        }

        return Task.FromResult(VKResult.Success());
    }
}
