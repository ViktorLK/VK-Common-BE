using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Domain and operational errors for the AI.Cortex building block.
/// Follows CS.01.
/// </summary>
public static class VKCortexErrors
{
    public static readonly VKError SessionNotFound =
        VKError.NotFound("Cortex.SessionNotFound", "The requested session thread was not found.");

    public static readonly VKError SessionExpired =
        VKError.Validation("Cortex.SessionExpired", "The session has expired due to idle timeout or boundary transition.");

    public static readonly VKError OrchestrationFailed =
        VKError.Failure("Cortex.OrchestrationFailed", "Failed to orchestrate dialogue turn execution.");

    public static readonly VKError ConsolidationFailed =
        VKError.Failure("Cortex.ConsolidationFailed", "Post-session multi-block consolidation coordination failed.");

    public static readonly VKError QuotaExceeded =
        VKError.Failure("Cortex.QuotaExceeded", "Operation aborted because the allocated token quota was exceeded.");
}
