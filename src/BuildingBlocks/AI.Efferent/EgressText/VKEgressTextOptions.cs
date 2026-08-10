using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent;


public sealed partial record VKEgressTextOptions : IVKToggleableBlockOptions
{
    public bool Enabled { get; init; } = true;
    public bool SanitizeMarkdown { get; init; } = true;
    public bool TrimWhitespace { get; init; } = true;

    // Human-like Pacing Settings
    public bool EnablePacing { get; init; } = true;
    public int BaseCharDelayMs { get; init; } = 35;
    public int SentenceEndDelayMs { get; init; } = 550;
    public int ClauseEndDelayMs { get; init; } = 250;
    public int ParagraphEndDelayMs { get; init; } = 700;
    public int InitialThinkingDelayMs { get; init; } = 200;
    public double JitterFactor { get; init; } = 0.15;
}
