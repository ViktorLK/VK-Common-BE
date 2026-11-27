namespace VK.Blocks.AI;

/// <summary>
/// Represents the three-state support status of an AI model capability.
/// </summary>
// [AIB-04] [AP.03]
public enum VKModelCapabilityState : byte
{
    /// <summary>
    /// Capability support is unknown (not explicitly declared in the catalog).
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Capability is explicitly supported by the model.
    /// </summary>
    Supported = 1,

    /// <summary>
    /// Capability is explicitly unsupported by the model.
    /// </summary>
    Unsupported = 2
}
