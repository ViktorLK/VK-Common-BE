using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Afferent;
using VK.Blocks.AI.Afferent.Tokenics.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Tokenics.Internal;

internal sealed class AfferentTokenicsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKAfferentTokenicsOptions _options;
    private readonly ILogger<AfferentTokenicsPipelineStage> _logger;

    public int StageOrder => 400; // Executes right before Framing

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentTokenicsPipelineStage(
        IVKTokenCounter tokenCounter,
        IOptionsSnapshot<VKAfferentTokenicsOptions> options,
        ILogger<AfferentTokenicsPipelineStage> logger)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.UserInput))
        {
            return Task.FromResult(VKResult.Success());
        }

        // Count tokens for UserInput
        int tokenCount;
        try
        {
            tokenCount = _tokenCounter.CountTokens(context.UserInput);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count tokens for UserInput");
            return Task.FromResult(VKResult.Failure(TokenicsErrors.CountingFailed));
        }

        // Check against MaxInputTokens
        if (tokenCount > _options.MaxInputTokens)
        {
            if (_options.EnforceHardLimit)
            {
                _logger.LogWarning("User input token count {Count} exceeds budget limit {Limit}.", tokenCount, _options.MaxInputTokens);
                return Task.FromResult(VKResult.Failure(TokenicsErrors.BudgetExceeded));
            }
        }

        // Check warning threshold
        float usageRatio = (float)tokenCount / _options.MaxInputTokens;
        if (usageRatio >= _options.BudgetWarningThreshold)
        {
            _logger.LogWarning("User input token count {Count} is close to budget limit {Limit} (Usage: {Ratio:P}).", tokenCount, _options.MaxInputTokens, usageRatio);
        }

        return Task.FromResult(VKResult.Success());
    }
}
