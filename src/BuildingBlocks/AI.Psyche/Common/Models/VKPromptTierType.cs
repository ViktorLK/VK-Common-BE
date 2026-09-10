// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the semantic layout tiers for prompt weaving assembly.
/// </summary>
public enum VKPromptTierType : byte
{
    /// <summary>
    /// Dynamic in-memory prompt fragments injected at runtime without persistent entity identity.
    /// Default fallback tier.
    /// </summary>
    Dynamic = 0,

    /// <summary>
    /// Directive-level instructions (e.g., system prompts, core rules).
    /// </summary>
    Directive = 1,

    /// <summary>
    /// Persona-specific definitions (e.g., character background, tone).
    /// </summary>
    Persona = 2,

    /// <summary>
    /// Injected knowledge context (e.g., RAG results, memory).
    /// </summary>
    Knowledge = 3,

    /// <summary>
    /// Custom prompt preset patterns woven into the prompt tapestry.
    /// </summary>
    Pattern = 4,

    /// <summary>
    /// User profile and session preferences context.
    /// </summary>
    Profile = 5
}
