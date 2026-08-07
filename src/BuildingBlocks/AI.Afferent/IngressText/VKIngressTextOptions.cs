using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;


public sealed partial record VKIngressTextOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool EnableUnicodeNormalization { get; init; } = true;
    public bool EnableWhitespaceTrimming { get; init; } = true;
    public int MaxInputLength { get; init; } = 100_000;
    public string NormalizationForm { get; init; } = "FormC";
}
