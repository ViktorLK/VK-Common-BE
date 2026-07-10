using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Persistence;

/// <summary>
/// Represents a persistence-agnostic outbox message.
/// </summary>
public sealed record VKOutboxMessage
{
    public required Guid Id { get; init; }
    public required string EventType { get; init; }
    public required string Payload { get; init; }
    public required DateTimeOffset OccurredOn { get; init; }
    public DateTimeOffset? ProcessedOn { get; init; }
    public int RetryCount { get; init; }
}
