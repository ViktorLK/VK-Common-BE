using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

public sealed partial record VKIdempotencyOptions : IVKBlockOptions
{
    public int ExpiryHours { get; init; } = 24;
}
