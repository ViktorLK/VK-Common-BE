using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Diagnostic EventId offsets for the AI.Psyche building block features.
/// Follows BB.04: Library-level sub-range allocation based on <see cref="VKDiagnosticOffsets.AI_Psyche"/>.
/// </summary>
public static class VKPsycheDiagnosticOffsets
{
    private const int Base = VKDiagnosticOffsets.AI_Psyche;

    public const int Behaviors = Base + 0;
    public const int Pattern = Base + 1000;
    public const int Directive = Base + 2000;
    public const int Persona = Base + 3000;
    public const int Knowledge = Base + 4000;
    public const int Echo = Base + 5000;
    public const int Weaving = Base + 6000;
    public const int Profile = Base + 7000;
    public const int Session = Base + 8000;
}

