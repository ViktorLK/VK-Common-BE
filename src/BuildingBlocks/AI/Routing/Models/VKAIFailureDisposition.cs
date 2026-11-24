namespace VK.Blocks.AI;

/// <summary>
/// Categorizes the failure disposition of an AI provider invocation for retry and failover decisions.
/// </summary>
public enum VKAIFailureDisposition : byte
{
    /// <summary>
    /// No failure disposition applies.
    /// </summary>
    None = 0,

    /// <summary>
    /// The request was rejected or failed before provider-side acceptance and execution.
    /// </summary>
    RequestNotAccepted = 1,

    /// <summary>
    /// The request was transmitted, and the provider-side execution outcome is unknown.
    /// </summary>
    OutcomeUnknown = 2,

    /// <summary>
    /// The request was terminally rejected and must not be retried or failed over unchanged.
    /// </summary>
    TerminalRejected = 3
}
