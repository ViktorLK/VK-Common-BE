namespace VK.Blocks.Core;

/// <summary>
/// Defines the strategy for request-level argument record generation.
/// </summary>
public enum VKArgsGenerationMode : byte
{
    /// <summary>
    /// Do not generate a request-level args record.
    /// </summary>
    None = 0,

    /// <summary>
    /// Generate a record containing only properties explicitly decorated with [VKRequestOverride].
    /// </summary>
    Explicit = 1,

    /// <summary>
    /// Generate a record containing all public properties except those decorated with [VKNoRequestOverride].
    /// </summary>
    Implicit = 2
}
