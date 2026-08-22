using System;

namespace VK.Blocks.Workflow;

/// <summary>
/// Composite query criteria for querying and filtering Workflow instances.
/// Follows AP.01.
/// </summary>
public sealed record VKWorkflowQueryFilter
{
    /// <summary>
    /// Optional workflow name filter.
    /// </summary>
    public string? WorkflowName { get; init; }

    /// <summary>
    /// Optional schema / definition version filter.
    /// </summary>
    public int? DefinitionVersion { get; init; }

    /// <summary>
    /// Optional parent workflow identifier filter for sub-workflows.
    /// </summary>
    public VKWorkflowId? ParentWorkflowId { get; init; }

    /// <summary>
    /// Optional status filter.
    /// </summary>
    public VKWorkflowState? State { get; init; }

    /// <summary>
    /// Optional correlation ID filter.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Optional lower bound creation time (inclusive).
    /// </summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>
    /// Optional upper bound creation time (inclusive).
    /// </summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    /// <summary>
    /// Pagination offset.
    /// </summary>
    public int Offset { get; init; } = 0;

    /// <summary>
    /// Pagination limit.
    /// </summary>
    public int Limit { get; init; } = 50;
}
