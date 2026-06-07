// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the semantic layout tiers for prompt weaving assembly.
/// </summary>
public enum VKPromptTierType
{
    /// <summary>
    /// Directive-level instructions (e.g., system prompts, core rules).
    /// </summary>
    Directive,

    /// <summary>
    /// Persona-specific definitions (e.g., character background, tone).
    /// </summary>
    Persona,

    /// <summary>
    /// Injected knowledge context (e.g., RAG results, memory).
    /// </summary>
    Knowledge,

    /// <summary>
    /// Immediate user interaction or specific task prompts.
    /// </summary>
    Echo,
}
