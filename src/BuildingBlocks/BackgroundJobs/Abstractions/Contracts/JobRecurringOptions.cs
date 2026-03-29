namespace VK.Blocks.BackgroundJobs.Abstractions.Contracts;

/// <summary>
/// Options for configuring a recurring job.
/// </summary>
public sealed record JobRecurringOptions(
    string? TimeZoneId = null,
    string? QueueName = null);
