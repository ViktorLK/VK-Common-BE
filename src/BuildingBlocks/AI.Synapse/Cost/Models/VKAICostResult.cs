namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Explicit result of an AI cost calculation, distinguishing priced models from unpriced ones.
/// </summary>
public readonly record struct VKAICostResult
{
    public bool IsPriced { get; init; }
    public decimal Amount { get; init; }

    public static VKAICostResult Unpriced => new() { IsPriced = false, Amount = 0m };

    public static VKAICostResult Priced(decimal amount) => new() { IsPriced = true, Amount = amount };
}
