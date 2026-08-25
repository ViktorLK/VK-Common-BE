namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines standard prompt insertion points inside the system instructions template.
/// </summary>
public enum VKPromptRelativeDepth : byte
{
    BeforeDirective = 0,
    AfterDirective = 1,
    BeforePersona = 2,
    AfterPersona = 3,
    BeforeEcho = 4,
    AfterEcho = 5
}
