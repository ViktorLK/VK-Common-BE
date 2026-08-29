using System;

namespace VK.Blocks.Validation;

/// <summary>
/// Controls the cascade mode (how rule execution behaves upon failure).
/// </summary>
public enum VKCascadeMode : byte
{
    /// <summary>
    /// Continue evaluating subsequent rules and collect all failures.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Stop evaluating further rules for the property or validator upon the first failure.
    /// </summary>
    Stop = 1
}
