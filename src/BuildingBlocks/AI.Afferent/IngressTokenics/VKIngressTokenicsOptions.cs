using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;


public sealed partial record VKIngressTokenicsOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public int MaxInputTokens { get; init; } = 32768;
    public float BudgetWarningThreshold { get; init; } = 0.8f;
    public bool EnforceHardLimit { get; init; } = true;
}
