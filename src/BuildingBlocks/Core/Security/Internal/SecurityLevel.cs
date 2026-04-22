namespace VK.Blocks.Core.Security.Internal;

/// <summary>
/// Defines the security level of a data property.
/// </summary>
internal enum SecurityLevel
{
    /// <summary>No special security handling required.</summary>
    None = 0,

    /// <summary>Contains sensitive data (PII) that should be masked (e.g., ***).</summary>
    Sensitive = 1,

    /// <summary>Contains highly sensitive data that should be fully redacted (hidden).</summary>
    Redacted = 2
}
