using System;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Persona.Internal;

internal sealed class DefaultPersonaRenderer : IVKPersonaRenderer
{
    public string Render(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona); // [AP.01]

        // High performance rendering path using VKValueStringBuilder with stackalloc buffer [CS.04]
        Span<char> initialBuffer = stackalloc char[512];
        using var sb = new VKValueStringBuilder(initialBuffer);

        if (!string.IsNullOrWhiteSpace(persona.Description))
        {
            sb.Append(PersonaConstants.MarkdownHeaders.Identity);
            sb.AppendLine();
            sb.AppendLine(persona.Description);
            sb.AppendLine();
        }

        if (persona.Traits is not null && persona.Traits.Count > 0)
        {
            sb.Append(PersonaConstants.MarkdownHeaders.Traits);
            sb.AppendLine();
            foreach (var trait in persona.Traits.OrderBy(t => t.Key, StringComparer.Ordinal))
            {
                sb.Append("- ");
                sb.Append(trait.Key);
                sb.Append(": ");
                sb.AppendLine(trait.Value);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
