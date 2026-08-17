using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Domain error constants for BackgroundJobs.
/// </summary>
public static class VKJobErrors
{
    public static VKError NotFound => VKError.NotFound("BackgroundJobs.Job.NotFound", "The specified background job was not found.");
    public static VKError ExecutionFailed => VKError.Failure("BackgroundJobs.Job.ExecutionFailed", "Background job execution failed.");
    public static VKError InvalidState => VKError.Validation("BackgroundJobs.Job.InvalidState", "Background job is in an invalid state.");
    public static VKError Timeout => VKError.Failure("BackgroundJobs.Job.Timeout", "Background job execution timed out.");
    public static VKError OutboxFailed => VKError.Failure("BackgroundJobs.Outbox.Failed", "Background job outbox operation failed.");
}
