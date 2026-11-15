using System;

namespace VK.Blocks.AI;

/// <summary>
/// Marks a class or method as an AI atomic tool for compile-time source generation.
/// Generates an <see cref="IVKAtomicTool"/> implementation with zero reflection (AP.01).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class VKAIToolAttribute : Attribute
{
    /// <summary>
    /// Gets the unique name of the tool. If null or empty, defaults to the class or method name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the description of what the tool does.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets or sets the functional category of the tool.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets comma-separated or list of tags associated with the tool.
    /// </summary>
    public string[] Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the tool only reads data without modifying state.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets whether the tool may perform destructive updates.
    /// </summary>
    public bool IsDestructive { get; set; }

    /// <summary>
    /// Gets or sets whether repeated calls with the same arguments produce no additional effect.
    /// </summary>
    public bool IsIdempotent { get; set; }

    /// <summary>
    /// Gets or sets whether the tool interacts with external untrusted entities (open world).
    /// </summary>
    public bool IsOpenWorld { get; set; }

    /// <summary>
    /// Gets or sets whether the tool is safe for concurrent execution.
    /// </summary>
    public bool IsConcurrencySafe { get; set; } = true;

    /// <summary>
    /// Gets or sets the approval requirement mode before executing the tool.
    /// </summary>
    public VKToolApprovalMode ApprovalMode { get; set; } = VKToolApprovalMode.Never;

    /// <summary>
    /// Gets or sets whether this tool requires explicit human confirmation before execution.
    /// </summary>
    public bool RequiresConfirmation
    {
        get => ApprovalMode != VKToolApprovalMode.Never;
        set => ApprovalMode = value ? VKToolApprovalMode.Always : VKToolApprovalMode.Never;
    }

    /// <summary>
    /// Gets or sets the specific execution timeout in milliseconds for this tool.
    /// </summary>
    public int TimeoutMilliseconds { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAIToolAttribute"/> class.
    /// </summary>
    /// <param name="name">Optional tool name.</param>
    /// <param name="description">Optional tool description.</param>
    public VKAIToolAttribute(string? name = null, string? description = null)
    {
        Name = name;
        Description = description;
    }
}

