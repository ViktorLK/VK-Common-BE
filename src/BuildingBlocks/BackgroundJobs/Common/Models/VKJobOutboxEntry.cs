using System;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Outbox storage entity for transactional job enqueueing.
/// </summary>
public sealed record VKJobOutboxEntry
{
    public required string Id { get; init; }
    public required string JobType { get; init; }
    public required string Payload { get; init; }
    public string Queue { get; init; } = "default";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public string? Error { get; init; }
    public bool IsProcessed => ProcessedAt.HasValue;
}
