using System.Linq;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

internal sealed class DefaultPersonaRenderer : IVKPersonaRenderer
{
    public string Render(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona);

        // High performance rendering path using StringBuilder with capacity estimation [AP.01]
        var sb = new StringBuilder(512);

        // 2. Knowledge Expression & Rule Trees (L3) -> Markdown
        sb.Append(PersonaConstants.MarkdownHeaders.Name).AppendLine().AppendLine(persona.Name).AppendLine();

        if (!string.IsNullOrWhiteSpace(persona.Description))
        {
            sb.Append(PersonaConstants.MarkdownHeaders.Identity).AppendLine().AppendLine(persona.Description).AppendLine();
        }

        if (persona.Traits is not null && persona.Traits.Any())
        {
            sb.Append(PersonaConstants.MarkdownHeaders.Traits).AppendLine();
            foreach (var trait in persona.Traits)
            {
                sb.Append("- ").Append(trait.Key).Append(": ").AppendLine(trait.Value);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
