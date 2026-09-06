namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Possible recovery actions determined by structured retry policy.
/// </summary>
public enum VKMaterializationRetryAction : byte
{
    /// <summary>
    /// No recovery action needed (validation succeeded).
    /// </summary>
    None = 0,

    /// <summary>
    /// Send corrective self-healing prompt to LLM within the same expression mode.
    /// </summary>
    PromptSelfHealing = 1,

    /// <summary>
    /// Accept partially valid payload based on tolerance mode.
    /// </summary>
    AcceptPartial = 2,

    /// <summary>
    /// Abort retries and fail gracefully with diagnostic error envelope.
    /// </summary>
    Abort = 3
}

/// <summary>
/// Evaluation decision returned by <see cref="IVKMaterializationRetryPolicy"/>.
/// Complies with [AP.01].
/// </summary>
public sealed record VKMaterializationRetryDecision // [AP.01]
{
    public required VKMaterializationRetryAction Action { get; init; }
    public VKAIEidosExpressionMode TargetMode { get; init; }
    public string? CorrectivePrompt { get; init; }
    public string? Reason { get; init; }
}
