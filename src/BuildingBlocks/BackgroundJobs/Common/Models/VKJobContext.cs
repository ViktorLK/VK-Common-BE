using System;
using System.Threading;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Execution context passed into background job handlers.
/// </summary>
public sealed record VKJobContext
{
    public required string JobId { get; init; }
    public string? TenantId { get; init; }
    public string? IdempotencyKey { get; init; }
    public int RetryCount { get; init; }
    public DateTimeOffset EnqueuedAt { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}
