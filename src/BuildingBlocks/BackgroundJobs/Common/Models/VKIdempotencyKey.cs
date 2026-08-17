using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Value object representing a job idempotency key.
/// </summary>
public sealed record VKIdempotencyKey
{
    public string Value { get; }

    public VKIdempotencyKey(string value)
    {
        Value = VKGuard.NotNullOrWhiteSpace(value);
    }

    public override string ToString() => Value;
}
