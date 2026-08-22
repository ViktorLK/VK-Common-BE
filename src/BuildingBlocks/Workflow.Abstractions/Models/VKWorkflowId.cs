using System;

namespace VK.Blocks.Workflow;

/// <summary>
/// Strong-typed unique identifier for a Workflow instance.
/// Follows AP.01 and CS.06.
/// </summary>
public readonly record struct VKWorkflowId : IEquatable<VKWorkflowId>
{
    public Guid Value { get; }

    public VKWorkflowId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("WorkflowId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static VKWorkflowId New() => new(Guid.NewGuid());

    public static VKWorkflowId FromGuid(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(VKWorkflowId id) => id.Value;
    public static implicit operator VKWorkflowId(Guid value) => new(value);
}
