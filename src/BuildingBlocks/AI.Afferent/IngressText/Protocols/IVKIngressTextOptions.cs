using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

public interface IVKIngressTextOptions : IVKToggleableBlockOptions
{
    bool EnableUnicodeNormalization { get; }
    bool EnableWhitespaceTrimming { get; }
    int MaxInputLength { get; }
    string NormalizationForm { get; }
}
