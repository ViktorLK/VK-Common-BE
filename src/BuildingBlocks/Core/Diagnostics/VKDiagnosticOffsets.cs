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
    public const int AI_Agents = 11000;
    public const int AI_AtomicTools = 12000;
    public const int AI_Audio = 13000;
    public const int AI_Chat = 14000;
    public const int AI_Guardrails = 15000;
    public const int AI_Prompting = 16000;
    public const int AI_Text = 17000;
    public const int AI_Tokenics = 18000;
    public const int AI_Vectorics = 19000;

    // ==========================================
    // AI.Afferent Block (20000 - 29999)
    // ==========================================
    public const int AI_Afferent_Weaving = 21000;
    public const int AI_Afferent_Knowledge = 22000;
    public const int AI_Afferent_Memory = 23000;
    public const int AI_Afferent_Persona = 24000;
    public const int AI_Afferent_Orchestration = 25000;
    public const int AI_Afferent_Presence = 26000;
    public const int AI_Afferent_Reasoning = 27000;
    public const int AI_Afferent_Framing = 28000;
    public const int AI_Afferent_Tokenics = 28500;
    public const int AI_Afferent_Guardrails = 29000;
    public const int AI_Afferent_Text = 29250;
    public const int AI_Afferent_Audio = 29500;
}
