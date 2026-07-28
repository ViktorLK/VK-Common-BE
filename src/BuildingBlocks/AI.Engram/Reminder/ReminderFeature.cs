using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Engram.Reminder.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Reminder feature marker and registration hub.
/// Follows BB.02 and BB.06.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKReminderOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ReminderFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKReminderOptions options)
    {
        services.TryAddSingleton<IVKReminderStore, InMemoryReminderStore>();
        services.TryAddScoped<IVKReminderService, DefaultReminderService>();
        services.TryAddScoped<DefaultReminderTriggerStage>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultReminderTriggerStage>());
        services.AddHostedService<ReminderScanBackgroundService>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKReminderOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.ScanIntervalSeconds <= 0)
        {
            failures.Add("VKReminderOptions.ScanIntervalSeconds must be greater than zero.");
        }

        if (options.DefaultExpiryDays <= 0)
        {
            failures.Add("VKReminderOptions.DefaultExpiryDays must be greater than zero.");
        }

        if (options.MaxSnoozeCount < 0)
        {
            failures.Add("VKReminderOptions.MaxSnoozeCount cannot be negative.");
        }

        if (options.DefaultSnoozeDurationMinutes <= 0)
        {
            failures.Add("VKReminderOptions.DefaultSnoozeDurationMinutes must be greater than zero.");
        }
    }
}
