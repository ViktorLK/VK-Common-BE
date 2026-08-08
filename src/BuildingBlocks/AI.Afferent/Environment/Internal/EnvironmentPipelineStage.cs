using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Environment.Internal;

/// <summary>
/// Pipeline stage for environment and screen perception.
/// </summary>
internal sealed class EnvironmentPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKEnvironmentPerceptionProvider _perceptionProvider;
    private readonly VKEnvironmentOptions _options;
    private readonly ILogger<EnvironmentPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(200, false, null, VKPipelinePhase.Before);

    public EnvironmentPipelineStage(
        IVKEnvironmentPerceptionProvider perceptionProvider,
        IOptionsSnapshot<VKEnvironmentOptions> options,
        ILogger<EnvironmentPipelineStage> logger)
    {
        _perceptionProvider = VKGuard.NotNull(perceptionProvider);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var result = await _perceptionProvider.GetEnvironmentStateAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to capture environment perception: {Error}", result.FirstError);
            // Non-fatal, we can proceed
            return VKResult.Success();
        }

        context.SetState(result.Value);
        return VKResult.Success();
    }
}
