using System.Collections.Generic;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Fake scheduler contract for unit testing assertions.
/// </summary>
public interface IVKFakeBackgroundJobScheduler : IVKBackgroundJobScheduler
{
    IReadOnlyList<VKFakeEnqueuedJob> EnqueuedJobs { get; }
    void Reset();
}
