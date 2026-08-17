namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Lifecycle states for background jobs.
/// </summary>
public enum VKJobState
{
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Deleted,
    DeadLetter
}
