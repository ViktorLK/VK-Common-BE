using System;
using System.Collections.Generic;

namespace VK.Blocks.Messaging;

/// <summary>
/// Represents the standard metadata envelope carried with every message.
/// </summary>
public sealed record VKMessageEnvelope
{
    public required Guid MessageId { get; init; }
    public required string MessageType { get; init; }
    public string? CorrelationId { get; init; }
    public string? CausationId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? TenantId { get; init; }
    public string? SchemaVersion { get; init; }
    public string? TraceContext { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
