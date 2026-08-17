using System;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Serialized payload container for job data and execution metadata.
/// </summary>
public sealed record VKJobPayload
{
    public required string JobType { get; init; }
    public required string SerializedData { get; init; }
    public string? TenantId { get; init; }
    public string? UserId { get; init; }
    public int SchemaVersion { get; init; } = 1;
    public DateTimeOffset CreatedAt { get; init; }
}
