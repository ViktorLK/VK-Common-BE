using System;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the policy for retrying failed message processing.
/// </summary>
public sealed record VKRetryPolicy
{
    public int MaxRetryCount { get; init; } = 3;
    public TimeSpan InitialInterval { get; init; } = TimeSpan.FromSeconds(1);
    public double BackoffExponent { get; init; } = 2.0;
}
