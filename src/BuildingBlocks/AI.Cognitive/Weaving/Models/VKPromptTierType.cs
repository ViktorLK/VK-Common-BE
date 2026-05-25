// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the semantic layout tiers for prompt weaving assembly.
/// Crucial for Roleplay (PWP) and complex Assistant layouts.
/// </summary>
public enum VKPromptTierType
{
    SystemInstructions = 0,
    Persona = 10,
    Scenario = 20,
    Knowledge = 30,
    ChatHistory = 40,
    AuthorNote = 50,
    Fallback = 99
}
