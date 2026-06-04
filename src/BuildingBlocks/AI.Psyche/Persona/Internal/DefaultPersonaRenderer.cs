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

        if (persona.OutputSpecification is not null)
        {
            sb.Append(PersonaConstants.MarkdownHeaders.OutputSpecification).AppendLine();

            sb.Append("- Format: ").AppendLine(persona.OutputSpecification.Format.ToString());

            if (persona.OutputSpecification.Format == VKResponseFormat.JsonSchema &&
                !string.IsNullOrWhiteSpace(persona.OutputSpecification.JsonSchemaDefinition))
            {
                sb.AppendLine("- JSON Schema:");
                sb.AppendLine("```json");
                sb.AppendLine(persona.OutputSpecification.JsonSchemaDefinition);
                sb.AppendLine("```");
            }

            if (!string.IsNullOrWhiteSpace(persona.OutputSpecification.IsoLanguageCode))
            {
                sb.Append("- Language: ").AppendLine(persona.OutputSpecification.IsoLanguageCode);
            }

            if (persona.OutputSpecification.MaxTokenHint > 0)
            {
                sb.Append("- Max Token Hint: ").AppendLine(persona.OutputSpecification.MaxTokenHint.ToString());
            }

            if (!string.IsNullOrWhiteSpace(persona.OutputSpecification.CustomConstraints))
            {
                sb.Append("- Custom Constraints: ").AppendLine(persona.OutputSpecification.CustomConstraints);
            }

            sb.AppendLine();
        }

        if (persona.FewShotExamples is not null && persona.FewShotExamples.Any())
        {
            sb.Append(PersonaConstants.MarkdownHeaders.FewShotExamples).AppendLine();
            for (int i = 0; i < persona.FewShotExamples.Count; i++)
            {
                var example = persona.FewShotExamples[i];
                sb.Append(PersonaConstants.MarkdownHeaders.ExamplePrefix).AppendLine((i + 1).ToString());
                sb.AppendLine(PersonaConstants.MarkdownHeaders.InputLabel);
                sb.AppendLine(example.Input);
                sb.AppendLine(PersonaConstants.MarkdownHeaders.ExpectedOutputLabel);
                sb.AppendLine(example.ExpectedOutput);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
