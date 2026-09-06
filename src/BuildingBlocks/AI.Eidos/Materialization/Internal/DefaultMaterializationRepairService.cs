using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Materialization.Internal;

internal sealed class DefaultMaterializationRepairService : IVKMaterializationRepairService // [AP.01]
{
    public string BuildRepairPrompt(
        VKMaterializationValidationResult validationResult,
        VKAIEidosSchema schema,
        int currentAttempt)
    {
        VKGuard.NotNull(validationResult);
        VKGuard.NotNull(schema);

        var sb = new StringBuilder();
        sb.AppendLine("CRITICAL INSTRUCTION: Your previous response failed contract validation.");
        sb.AppendLine("Please fix the following validation errors and return ONLY the corrected, valid raw JSON matching the required schema:");
        sb.AppendLine();

        var idx = 1;
        foreach (var error in validationResult.Errors)
        {
            sb.Append(idx++).Append(". ");
            if (!string.IsNullOrWhiteSpace(error.PropertyPath))
            {
                sb.Append("Property '").Append(error.PropertyPath).Append("': ");
            }
            sb.Append(error.Message);

            if (!string.IsNullOrWhiteSpace(error.Expected))
            {
                sb.Append(" (Expected: ").Append(error.Expected).Append(')');
            }

            if (!string.IsNullOrWhiteSpace(error.Actual))
            {
                sb.Append(" (Got: ").Append(error.Actual).Append(')');
            }

            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Target JSON Schema:");
        sb.AppendLine(schema.RawJsonSchema);

        return sb.ToString();
    }
}
