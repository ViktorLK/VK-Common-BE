using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Predefined error constants and factory helpers for the Resilience building block.
/// </summary>
public static class VKResilienceErrors
{
    /// <summary>
    /// Operation timed out.
    /// </summary>
    public static readonly VKError Timeout = new("Resilience.Timeout", "The operation timed out.");

    /// <summary>
    /// Operation execution failed.
    /// </summary>
    public static readonly VKError ExecutionFailed = new("Resilience.ExecutionFailed", "The operation execution failed.");

    /// <summary>
    /// Retry attempts exhausted.
    /// </summary>
    public static readonly VKError RetryExhausted = new("Resilience.RetryExhausted", "Retry attempts exhausted.");

    /// <summary>
    /// Fallback execution failed.
    /// </summary>
    public static readonly VKError FallbackFailed = new("Resilience.FallbackFailed", "Primary and fallback actions failed.");

    /// <summary>
    /// Circuit breaker is open.
    /// </summary>
    public static readonly VKError CircuitBreakerOpen = new("Resilience.CircuitBreakerOpen", "Circuit breaker is open for the specified key.");

    /// <summary>
    /// Rate limit exceeded.
    /// </summary>
    public static readonly VKError RateLimitExceeded = new("Resilience.RateLimitExceeded", "Rate limit exceeded for the specified key.");

    /// <summary>
    /// Bulkhead concurrency limit exceeded.
    /// </summary>
    public static readonly VKError BulkheadExceeded = new("Resilience.BulkheadExceeded", "Bulkhead concurrency capacity exceeded for the specified key.");

    /// <summary>
    /// Creates a timeout error with specific duration details.
    /// </summary>
    public static VKError CreateTimeout(double durationMs) =>
        new("Resilience.Timeout", $"Operation timed out after {durationMs}ms.");

    /// <summary>
    /// Creates an execution failure error with message details.
    /// </summary>
    public static VKError CreateExecutionFailed(string message) =>
        new("Resilience.ExecutionFailed", message);

    /// <summary>
    /// Creates a retry exhausted error with message details.
    /// </summary>
    public static VKError CreateRetryExhausted(string? message) =>
        new("Resilience.RetryExhausted", message ?? "Retry attempts exhausted.");

    /// <summary>
    /// Creates a fallback failed error with message details.
    /// </summary>
    public static VKError CreateFallbackFailed(string fallbackMessage) =>
        new("Resilience.FallbackFailed", $"Primary and fallback actions failed: {fallbackMessage}");
}
