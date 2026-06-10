namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines standard prompt insertion points inside the system instructions template.
/// </summary>
public enum VKPromptRelativeAnchor
{
    BeforeDirective,
    AfterDirective,
    BeforePersona,
    AfterPersona,
    BeforeEcho,
    AfterEcho
}
