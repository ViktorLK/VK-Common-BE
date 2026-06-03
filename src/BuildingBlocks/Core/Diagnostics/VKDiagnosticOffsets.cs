namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Global registry for EventId offsets across the entire VK.Blocks ecosystem.
/// Prevents EventId collisions and provides a centralized view of allocated ranges.
/// </summary>
public static class VKDiagnosticOffsets
{
    // ==========================================
    // AI Block (10000 - 19999)
    // ==========================================
    public const int AI_Agents       = 11000;
    public const int AI_AtomicTools  = 12000;
    public const int AI_Audio        = 13000;
    public const int AI_Chat         = 14000;
    public const int AI_Guardrails   = 15000;
    public const int AI_Prompting    = 16000;
    public const int AI_Text         = 17000;
    public const int AI_Tokenics     = 18000;
    public const int AI_Vectorics    = 19000;

    // ==========================================
    // AI.Cognitive Block (20000 - 29999)
    // ==========================================
    public const int AI_Cognitive_Weaving       = 21000;
    public const int AI_Cognitive_Knowledge     = 22000;
    public const int AI_Cognitive_Memory        = 23000;
    public const int AI_Cognitive_Persona       = 24000;
    public const int AI_Cognitive_Orchestration = 25000;
    public const int AI_Cognitive_Presence      = 26000;
    public const int AI_Cognitive_Reasoning     = 27000;
    public const int AI_Cognitive_Framing       = 28000;
}
