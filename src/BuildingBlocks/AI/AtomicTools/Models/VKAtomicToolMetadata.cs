using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the structural metadata for an atomic tool.
/// Following the "Industrial DNA" (Typed Metadata Pattern).
/// </summary>
public sealed record VKAtomicToolMetadata
{
    /// <summary>
    /// Gets the name of the tool.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of what the tool does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the category of the tool.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the list of tags associated with the tool.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the tool only reads data without modifying state.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tool may perform destructive updates.
    /// </summary>
    public bool IsDestructive { get; init; }

    /// <summary>
    /// Gets a value indicating whether repeated calls with the same arguments produce no additional effect.
    /// </summary>
    public bool IsIdempotent { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tool interacts with external untrusted entities (open world).
    /// </summary>
    public bool IsOpenWorld { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tool is safe for concurrent execution.
    /// </summary>
    public bool IsConcurrencySafe { get; init; } = true;

    /// <summary>
    /// Gets the provenance trust level of the tool.
    /// </summary>
    public VKToolTrustLevel TrustLevel { get; init; } = VKToolTrustLevel.Trusted;

    /// <summary>
    /// Gets the approval requirement mode before executing the tool.
    /// </summary>
    public VKToolApprovalMode ApprovalMode { get; init; } = VKToolApprovalMode.Never;

    /// <summary>
    /// Gets a value indicating whether the tool execution requires explicit human confirmation.
    /// </summary>
    public bool RequiresConfirmation
    {
        get => ApprovalMode != VKToolApprovalMode.Never;
        init => ApprovalMode = value ? VKToolApprovalMode.Always : VKToolApprovalMode.Never;
    }

    /// <summary>
    /// Gets the specific timeout for this tool in milliseconds.
    /// </summary>
    public int? TimeoutMilliseconds { get; init; }
}
