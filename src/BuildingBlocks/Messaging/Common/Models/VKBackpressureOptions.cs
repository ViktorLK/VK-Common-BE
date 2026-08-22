namespace VK.Blocks.Messaging;

/// <summary>
/// Configures backpressure and concurrency limits for consumers.
/// </summary>
public sealed record VKBackpressureOptions
{
    public int PrefetchCount { get; init; } = 16;
    public int ConcurrentConsumerLimit { get; init; } = 8;
}
