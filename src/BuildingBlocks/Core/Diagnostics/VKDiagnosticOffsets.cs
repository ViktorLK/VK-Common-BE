namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Global registry for EventId offsets across the entire VK.Blocks ecosystem.
/// Prevents EventId collisions and provides a centralized view of allocated ranges.
/// </summary>
public static class VKDiagnosticOffsets
{
    // ==========================================
    // AI Block (50000 - 69999)
    // ==========================================
    public const int AI_Agents = 51000;
    public const int AI_AtomicTools = 52000;
    public const int AI_Audio = 53000;
    public const int AI_Chat = 54000;
    public const int AI_Guardrails = 55000;
    public const int AI_Prompting = 56000;
    public const int AI_Text = 57000;
    public const int AI_Tokenics = 58000;
    public const int AI_Vectorics = 59000;

    // ==========================================
    // AI.Psyche Block (70000 - 89999)
    // ==========================================
    public const int AI_Psyche_Behaviors = 70000;
    public const int AI_Psyche_Pattern = 71000;
    public const int AI_Psyche_Directive = 72000;
    public const int AI_Psyche_Persona = 73000;
    public const int AI_Psyche_Knowledge = 74000;
    public const int AI_Psyche_Echo = 75000;
    public const int AI_Psyche_Weaving = 76000;
}
