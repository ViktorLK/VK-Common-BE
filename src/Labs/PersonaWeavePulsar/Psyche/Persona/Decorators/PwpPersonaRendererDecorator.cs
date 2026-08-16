using System.Text;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;
using VK.Labs.PersonaWeavePulsar.Psyche.Extensions;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Decorators;

/// <summary>
/// A decorator over the default persona renderer.
/// It delegates the core properties to the base renderer and then appends PWP-specific roleplay Markdown.
/// </summary>
public sealed class PwpPersonaRendererDecorator : IVKPersonaRenderer
{
    private readonly IVKPersonaRenderer _innerRenderer;

    public PwpPersonaRendererDecorator(IVKPersonaRenderer innerRenderer)
    {
        _innerRenderer = VKGuard.NotNull(innerRenderer);
    }

    public string Render(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona);

        // 1. Let the core renderer handle the standard Markdown (Name, Description, Traits, OutputSpec)
        var basePrompt = _innerRenderer.Render(persona);

        var sb = new StringBuilder();

        // 2. Extract PWP specific extensions and format as Markdown (COG07: L3 Knowledge Expression)
        var personality = persona.GetPersonality();
        if (!string.IsNullOrWhiteSpace(personality))
        {
            sb.Append("# Personality\n").AppendLine(personality).AppendLine();
        }

        var scenario = persona.GetScenario();
        if (!string.IsNullOrWhiteSpace(scenario))
        {
            sb.Append("# Scenario\n").AppendLine(scenario).AppendLine();
        }

        var firstMessage = persona.GetFirstMessage();
        if (!string.IsNullOrWhiteSpace(firstMessage))
        {
            sb.Append("# First Message\n").AppendLine(firstMessage).AppendLine();
        }

        var dialogues = persona.GetDialogueExamples();
        if (!string.IsNullOrWhiteSpace(dialogues))
        {
            sb.Append("# Dialogue Examples\n").AppendLine(dialogues).AppendLine();
        }

        // 3. Append to the base prompt
        if (sb.Length > 0)
        {
            return basePrompt + "\n" + sb.ToString();
        }

        return basePrompt;
    }
}
