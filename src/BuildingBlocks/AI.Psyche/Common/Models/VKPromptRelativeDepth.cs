namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines standard prompt insertion points across directive, persona, echo history, and user input pillars.
/// </summary>
public enum VKPromptRelativeDepth : byte
{
    BeforeDirective = 0,
    AfterDirective = 1,
    BeforePersona = 2,
    AfterPersona = 3,
    BeforeEcho = 4,
    AfterEcho = 5,
    BeforeInput = 6,
    AfterInput = 7
}
