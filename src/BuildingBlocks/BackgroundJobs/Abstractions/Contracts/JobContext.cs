namespace VK.Blocks.BackgroundJobs.Abstractions.Contracts;

/// <summary>
/// Context for job execution.
/// </summary>
/// <param name="JobId">The unique identifier of the job.</param>
/// <param name="TenantId">The tenant identifier associated with the job.</param>
/// <param name="CorrelationId">The correlation identifier for tracing.</param>
public record JobContext(string JobId, string? TenantId, string? CorrelationId);
