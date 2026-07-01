using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressTokenicsOptions : IVKToggleableBlockOptions
{
    int MaxInputTokens { get; }
    float BudgetWarningThreshold { get; }
    bool EnforceHardLimit { get; }
}
