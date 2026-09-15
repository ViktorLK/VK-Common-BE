namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines standard prompt relative insertion anchors for static prompt layout (Directive and Persona).
/// </summary>
public enum VKPromptRelativeDepth : byte
{
    BeforeDirective = 0,
    AfterDirective = 1,
    BeforePersona = 2,
    AfterPersona = 3
}
