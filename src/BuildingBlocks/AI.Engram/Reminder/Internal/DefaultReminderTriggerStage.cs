using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Reminder.Internal;

/// <summary>
/// Pipeline stage running BEFORE the LLM call to evaluate and trigger active prospective reminders.
/// </summary>
internal sealed class DefaultReminderTriggerStage : IVKPsychePipelineStage
{
    private readonly IVKReminderService _reminderService;
    private readonly VKReminderOptions _options;

    public DefaultReminderTriggerStage(IVKReminderService reminderService, IOptions<VKReminderOptions> options)
    {
        // // [AP.01] Fluent guard assignment
        _reminderService = VKGuard.NotNull(reminderService);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.CorpusFiltering;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        // // [AP.01] Boundary guard check
        VKGuard.NotNull(context);

        // // [CS.03] Async call with ConfigureAwait(false)
        return await _reminderService.EvaluateRemindersAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
