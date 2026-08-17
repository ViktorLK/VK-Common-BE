namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Diagnostic constants for telemetry and observability.
/// </summary>
public static class VKBackgroundJobsDiagnosticsConstants
{
    public const string BlockName = "BackgroundJobs";
    public const string JobsEnqueuedMeter = "vk.backgroundjobs.jobs_enqueued";
    public const string JobsExecutedMeter = "vk.backgroundjobs.jobs_executed";
    public const string JobsFailedMeter = "vk.backgroundjobs.jobs_failed";
}
