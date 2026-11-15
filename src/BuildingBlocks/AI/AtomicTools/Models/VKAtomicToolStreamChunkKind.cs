namespace VK.Blocks.AI;

/// <summary>
/// Defines the category of a streaming chunk emitted by an atomic tool.
/// </summary>
public enum VKAtomicToolStreamChunkKind : byte
{
    Text = 0,
    Progress = 1,
    NeedsInput = 2
}

