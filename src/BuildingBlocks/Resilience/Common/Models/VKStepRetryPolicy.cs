using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Step-level retry policy configuration for classifying errors and controlling backoff delays.
/// Follows [AP.01] and [BB.05].
/// </summary>
public sealed record VKStepRetryPolicy
{
    /// <summary>
    /// Gets the maximum number of retry attempts for transient errors.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets the base / initial delay in milliseconds for backoff.
    /// </summary>
    public int InitialDelayMs { get; init; } = 500;

    /// <summary>
    /// Gets the multiplier used for exponential backoff calculations.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Gets a value indicating whether to apply jitter to backoff delays.
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Custom predicate to determine whether an error is transient (retriable).
    /// If null, default transient rules (e.g., Timeout, 429, 5xx, or network failure) apply.
    /// </summary>
    public Func<VKError, bool>? IsTransientPredicate { get; init; }

    /// <summary>
    /// Determines whether the specified error is transient based on the policy rules.
    /// </summary>
    public bool IsTransient(VKError error)
    {
        if (IsTransientPredicate is not null)
        {
            return IsTransientPredicate(error);
        }

        // Default heuristic: Treat 429, timeout, network failure, or transient error codes as transient
        var code = error.Code;
        if (string.IsNullOrEmpty(code))
        {
            return false;
        }

        return code.Contains("Timeout", StringComparison.OrdinalIgnoreCase)
            || code.Contains("Transient", StringComparison.OrdinalIgnoreCase)
            || code.Contains("RateLimit", StringComparison.OrdinalIgnoreCase)
            || code.Contains("429", StringComparison.OrdinalIgnoreCase)
            || code.Contains("503", StringComparison.OrdinalIgnoreCase)
            || code.Contains("504", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates the backoff delay for the specified retry attempt.
    /// </summary>
    public TimeSpan CalculateDelay(int attempt)
    {
        if (attempt <= 1)
        {
            return TimeSpan.FromMilliseconds(InitialDelayMs);
        }

        var delayMs = InitialDelayMs * Math.Pow(BackoffMultiplier, attempt - 1);
        return TimeSpan.FromMilliseconds(Math.Min(delayMs, 60_000)); // Cap at 60 seconds
    }

    /// <summary>
    /// Default retry policy: 3 retries, exponential backoff starting at 500ms.
    /// </summary>
    public static VKStepRetryPolicy Default { get; } = new();

    /// <summary>
    /// No retry policy: Fail immediately on any error.
    /// </summary>
    public static VKStepRetryPolicy None { get; } = new() { MaxRetries = 0 };
}
