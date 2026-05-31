namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines standard prompt insertion points inside the system instructions template.
/// Complies with AP.01 and AP.03.
/// </summary>
public enum VKKnowledgeRelative
{
    BeforeDirective,
    AfterDirective,
    BeforePersona,
    AfterPersona,
    BeforeEcho,
    AfterEcho
}
