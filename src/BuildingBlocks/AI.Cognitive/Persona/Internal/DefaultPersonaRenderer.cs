using System.Linq;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

internal sealed class DefaultPersonaRenderer : IVKPersonaRenderer
{
    public string Render(VKPersonaAnchor persona)
    {
        VKGuard.NotNull(persona);

        // High performance rendering path using StringBuilder with capacity estimation [AP.01]
        var sb = new StringBuilder(512);

        // 2. Knowledge Expression & Rule Trees (L3) -> Markdown
        sb.Append("# Name\n").AppendLine(persona.Name).AppendLine();

        if (!string.IsNullOrWhiteSpace(persona.Description))
        {
            sb.Append("# Background\n").AppendLine(persona.Description).AppendLine();
        }

        if (persona.Traits is not null && persona.Traits.Any())
        {
            sb.Append("# Traits\n");
            foreach (var trait in persona.Traits)
            {
                sb.Append("- ").Append(trait.Key).Append(": ").AppendLine(trait.Value);
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(persona.SystemDirectives))
        {
            sb.Append("# System Directives\n").AppendLine(persona.SystemDirectives).AppendLine();
        }

        if (persona.OutputSpecification is not null)
        {
            sb.Append("# Output Specification\n");

            if (persona.OutputSpecification.Format != VKResponseFormat.Unspecified)
            {
                sb.Append("- Format: ").AppendLine(persona.OutputSpecification.Format.ToString());

                if (persona.OutputSpecification.Format == VKResponseFormat.JsonSchema &&
                    !string.IsNullOrWhiteSpace(persona.OutputSpecification.JsonSchemaDefinition))
                {
                    sb.AppendLine("- JSON Schema:");
                    sb.AppendLine("```json");
                    sb.AppendLine(persona.OutputSpecification.JsonSchemaDefinition);
                    sb.AppendLine("```");
                }
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
            sb.Append("# Few-Shot Examples\n");
            for (int i = 0; i < persona.FewShotExamples.Count; i++)
            {
                var example = persona.FewShotExamples[i];
                sb.Append("## Example ").AppendLine((i + 1).ToString());
                sb.AppendLine("**Input:**");
                sb.AppendLine(example.Input);
                sb.AppendLine("**Expected Output:**");
                sb.AppendLine(example.ExpectedOutput);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
