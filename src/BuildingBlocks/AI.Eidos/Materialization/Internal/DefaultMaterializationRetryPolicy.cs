using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Materialization.Internal;

internal sealed class DefaultMaterializationRetryPolicy(
    IVKMaterializationRepairService repairService) : IVKMaterializationRetryPolicy // [AP.01]
{
    private readonly IVKMaterializationRepairService _repairService = VKGuard.NotNull(repairService);

    public VKMaterializationRetryDecision Evaluate(
        int currentAttempt,
        VKMaterializationValidationResult validationResult,
        VKAIEidosSchema schema,
        VKAIEidosExpressionMode currentMode,
        VKMaterializationOptions options)
    {
        VKGuard.NotNull(validationResult);
        VKGuard.NotNull(schema);
        VKGuard.NotNull(options);

        if (validationResult.IsValid)
        {
            return new VKMaterializationRetryDecision
            {
                Action = VKMaterializationRetryAction.None,
                TargetMode = currentMode
            };
        }

        // Tier 1: Auto-Repair in same mode
        if (options.EnableAutoRepair && currentAttempt < options.MaxRepairAttempts)
        {
            var correctivePrompt = _repairService.BuildRepairPrompt(validationResult, schema, currentAttempt);
            return new VKMaterializationRetryDecision
            {
                Action = VKMaterializationRetryAction.PromptSelfHealing,
                TargetMode = currentMode,
                CorrectivePrompt = correctivePrompt,
                Reason = $"Auto-repair attempt {currentAttempt + 1} of {options.MaxRepairAttempts}"
            };
        }

        // Tier 2: Partial acceptance check before abort
        if (options.ToleranceMode == VKMaterializationToleranceMode.Partial)
        {
            return new VKMaterializationRetryDecision
            {
                Action = VKMaterializationRetryAction.AcceptPartial,
                TargetMode = currentMode,
                Reason = "Accepting partial payload under Partial tolerance mode."
            };
        }

        // Tier 3: Abort
        return new VKMaterializationRetryDecision
        {
            Action = VKMaterializationRetryAction.Abort,
            TargetMode = currentMode,
            Reason = "All repair attempts exhausted."
        };
    }
}
