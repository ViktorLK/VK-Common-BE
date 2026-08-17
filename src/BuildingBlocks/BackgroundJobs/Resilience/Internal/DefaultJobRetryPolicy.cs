using System;

namespace VK.Blocks.BackgroundJobs.Resilience.Internal;

internal sealed class DefaultJobRetryPolicy
{
    public TimeSpan CalculateBackoff(int currentAttempt, int baseSeconds = 2)
    {
        return TimeSpan.FromSeconds(Math.Pow(baseSeconds, currentAttempt));
    }
}
