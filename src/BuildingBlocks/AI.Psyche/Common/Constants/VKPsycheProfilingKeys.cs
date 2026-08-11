using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public constants defining standardized metric key names for profiling durations across Psyche pipeline stages and tasks.
/// Prevents magic string drift in telemetry, observability, and response evaluation.
/// Follows AP.03 (VK prefix).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static constants class without logic.")]
public static class VKPsycheProfilingKeys
{
    public const string SessionResolveStage = "SessionResolveStage";
    public const string SessionUpdateStage = "SessionUpdateStage";
    public const string ProfileStage = "ProfileStage";
    public const string DirectiveStage = "DirectiveStage";
    public const string PersonaStage = "PersonaStage";
    public const string EchoExtractStage = "EchoExtractStage";
    public const string EchoSaveStage = "EchoSaveStage";
    public const string KnowledgeStage = "KnowledgeStage";
    public const string PatternStage = "PatternStage";
    public const string WeavingStage = "WeavingStage";
    public const string LLMInvocation = "LLMInvocation";
}
