namespace VK.Blocks.Workflow;

/// <summary>
/// Strategy contract for computing deterministic idempotency keys for Workflow executions.
/// </summary>
public interface IVKIdempotencyKeyGenerator
{
    /// <summary>
    /// Computes a deterministic idempotency key from a workflow name and context payload.
    /// </summary>
    string GenerateKey<TContext>(string workflowName, TContext context);
}
