using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Represents the execution context for a resilience operation.
/// Follows [AP.01], [CS.01], [CS.06].
/// </summary>
public sealed record VKResilienceContext
{
    /// <summary>
    /// Gets the unique operation key (e.g., "llm-turn", "http-client-call").
    /// </summary>
    public required string OperationKey { get; init; }

    /// <summary>
    /// Gets the distributed trace identifier if available.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the tenant identifier for multi-tenant isolation.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the logical operation name.
    /// </summary>
    public string? OperationName { get; init; }

    /// <summary>
    /// Gets a dictionary of custom properties for the context.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Creates a new <see cref="VKResilienceContext"/> instance with standard parameters.
    /// </summary>
    public static VKResilienceContext Create(
        string operationKey,
        string? traceId = null,
        string? tenantId = null,
        string? operationName = null,
        IReadOnlyDictionary<string, object>? properties = null)
    {
        VKGuard.NotNullOrWhiteSpace(operationKey);

        return new VKResilienceContext
        {
            OperationKey = operationKey,
            TraceId = traceId,
            TenantId = tenantId,
            OperationName = operationName,
            Properties = properties ?? new Dictionary<string, object>()
        };
    }
}
