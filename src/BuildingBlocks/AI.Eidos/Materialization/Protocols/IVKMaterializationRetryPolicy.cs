namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Policy governing multi-tier retry, self-healing, and mode fallback decisions for AI materialization lifecycle.
/// </summary>
public interface IVKMaterializationRetryPolicy
{
    /// <summary>
    /// Evaluates current materialization state and determines the next recovery action.
    /// </summary>
    VKMaterializationRetryDecision Evaluate(
        int currentAttempt,
        VKMaterializationValidationResult validationResult,
        VKAIEidosSchema schema,
        VKAIEidosExpressionMode currentMode,
        VKMaterializationOptions options);
}
