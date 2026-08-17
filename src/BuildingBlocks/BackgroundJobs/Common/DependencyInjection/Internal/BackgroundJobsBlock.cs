using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.BackgroundJobs.Concurrency.Internal;
using VK.Blocks.BackgroundJobs.Idempotency.Internal;
using VK.Blocks.BackgroundJobs.Jobs.Internal;
using VK.Blocks.BackgroundJobs.Management.Internal;
using VK.Blocks.BackgroundJobs.Outbox.Internal;
using VK.Blocks.BackgroundJobs.Recurring.Internal;
using VK.Blocks.BackgroundJobs.Resilience.Internal;
using VK.Blocks.BackgroundJobs.Shared;
using VK.Blocks.BackgroundJobs.StateTracking.Internal;

namespace VK.Blocks.BackgroundJobs;

public sealed partial class VKBackgroundJobsBlock
{
    static partial void RegisterBlockCustom(IVKBackgroundJobsBuilder builder)
    {
        var services = builder.Services;

        services.TryAddSingleton<JobPayloadSerializer>();
        services.TryAddSingleton<TenantContextRestorer>();

        services.TryAddSingleton<IVKJobStore, BasicJobStore>();
        services.TryAddSingleton<IVKJobStateStore, BasicJobStateStore>();
        services.TryAddSingleton<IVKJobIdempotencyService, DefaultJobIdempotencyService>();
        services.TryAddSingleton<IVKJobConcurrencyLimiter, DefaultJobConcurrencyLimiter>();
        services.TryAddSingleton<IVKDeadLetterStore, BasicDeadLetterStore>();
        services.TryAddSingleton<IVKJobDistributedLock, NoOpJobDistributedLock>();

        services.TryAddSingleton<IVKBackgroundJobScheduler, DefaultBackgroundJobScheduler>();
        services.TryAddSingleton<IVKRecurringJobScheduler, DefaultRecurringJobScheduler>();
        services.TryAddSingleton<IVKJobOutbox, DefaultJobOutbox>();
        services.TryAddSingleton<IVKJobManagementService, DefaultJobManagementService>();

        services.AddHostedService<DefaultJobOutboxProcessor>();
    }
}
