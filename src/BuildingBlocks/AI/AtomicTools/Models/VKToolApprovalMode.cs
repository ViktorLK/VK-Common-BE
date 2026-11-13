namespace VK.Blocks.AI;

/// <summary>
/// Defines the approval requirement mode before executing an atomic tool.
/// </summary>
public enum VKToolApprovalMode : byte
{
    Never = 0,
    Always = 1,
    ByPolicy = 2
}

