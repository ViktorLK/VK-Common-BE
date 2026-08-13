using System;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Parsing.Internal;

internal sealed class DefaultContractRepairService : IVKContractRepairService
{
    public VKAIEidosRepairInstruction BuildRepairInstruction(
        VKAIEidosValidationResult validationResult,
        VKAIEidosSchema schema,
        int currentAttempt)
    {
        VKGuard.NotNull(validationResult);
        VKGuard.NotNull(schema);

        Span<char> initialBuffer = stackalloc char[256];
        using var sb = new VKValueStringBuilder(initialBuffer);
        sb.AppendLine("Your previous response failed JSON Contract Validation:");
        foreach (var err in validationResult.ErrorMessages)
        {
            sb.AppendLine($"- {err}");
        }

        sb.AppendLine("Please fix your output and strictly follow this JSON Schema:");
        sb.AppendLine(schema.RawJsonSchema);

        return new VKAIEidosRepairInstruction
        {
            CorrectivePrompt = sb.ToString(),
            AttemptCount = currentAttempt + 1
        };
    }
}
