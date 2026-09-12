using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Common.Internal;
using VK.Blocks.AI.Psyche.Directive.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Directive.Internal;

/// <summary>
/// Default implementation of the Psyche Directive Stage.
/// Injects system guardrails, safety rules, and operational constraints into the system prompt.
/// </summary>
[VKTrace("psyche.stage.directive")]
internal sealed class DefaultDirectiveStage : IVKPsychePipelineStage
{
    private readonly VKDirectiveOptions _options;
    private readonly IVKPsycheDirectiveRepository _directiveRepository;
    private readonly IVKDirectiveRenderer _directiveRenderer;
    private readonly ILogger<DefaultDirectiveStage> _logger;

    public DefaultDirectiveStage(
        VKDirectiveOptions options,
        IVKPsycheDirectiveRepository directiveRepository,
        IVKDirectiveRenderer directiveRenderer,
        ILogger<DefaultDirectiveStage> logger)
    {
        _options = VKGuard.NotNull(options);
        _directiveRepository = VKGuard.NotNull(directiveRepository);
        _directiveRenderer = VKGuard.NotNull(directiveRenderer);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheDirective;
    public bool IsActive => _options.Enabled;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var isEnabled = context.Args<VKDirectiveArgs>()?.Enabled ?? _options.Enabled;
        if (!isEnabled)
        {
            return VKResult.Success();
        }

        if (context.Request.DirectiveIds.Count == 0)
        {
            return VKResult.Success();
        }

        var resolveResult = await _directiveRepository.ListByIdsAsync(context.Request.DirectiveIds, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (resolveResult.IsFailure)
        {
            return VKResult.Failure(resolveResult.Errors);
        }

        // Dual-factor deterministic sorting: Priority first (ascending), then request declaration order
        var requestOrder = context.Request.DirectiveIds
            .Select((id, index) => (id, index))
            .ToDictionary(x => x.id, x => x.index);

        var sortedDirectives = resolveResult.Value
            .OrderBy(d => d.Priority)
            .ThenBy(d => requestOrder.GetValueOrDefault(d.Id, int.MaxValue))
            .ToList();

        context.SetState(sortedDirectives);

        for (int i = 0; i < sortedDirectives.Count; i++)
        {
            var directive = sortedDirectives[i];
            _logger.DirectiveResolved(directive.Id);

            var content = _directiveRenderer.Render(directive);
            if (!string.IsNullOrWhiteSpace(content))
            {
                context.AddSegment(new VKPromptSegment
                {
                    Tier = VKPromptTierType.Directive,
                    TagName = PsycheConstants.XmlTags.SystemDirective,
                    Content = content,
                    DepthPriority = i,
                    TokenCount = directive.TokenCount
                });
            }
        }

        if (resolveResult.Value.Count > 0)
        {
            DirectiveDiagnostics.RecordDirectivesResolved(resolveResult.Value.Count, "Directive");
        }

        return VKResult.Success();
    }
}
