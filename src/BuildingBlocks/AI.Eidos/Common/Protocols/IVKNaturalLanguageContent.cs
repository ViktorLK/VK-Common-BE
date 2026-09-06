namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Defines an optional contract allowing structured output models (DTOs) to project themselves
/// into a natural language text representation for multi-turn dialogue history (Echo) and speech delivery.
/// Follows AP.03 (Level 1 interface in module root namespace with VK prefix).
/// </summary>
public interface IVKNaturalLanguageContent
{
    /// <summary>
    /// Projects the structured output model into a natural language dialogue string.
    /// </summary>
    /// <returns>The natural language representation of the response.</returns>
    string ToNaturalLanguage();
}
