namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the runtime state of a circuit breaker.
/// Follows [AP.01].
/// </summary>
public enum VKCircuitState : byte
{
    /// <summary>
    /// Normal operation: Executions are allowed, failures are monitored.
    /// </summary>
    Closed = 0,

    /// <summary>
    /// Tripped: Executions are blocked during the cooldown duration.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Recovery probation: A limited number of trial executions are permitted to probe service health.
    /// </summary>
    HalfOpen = 2
}
