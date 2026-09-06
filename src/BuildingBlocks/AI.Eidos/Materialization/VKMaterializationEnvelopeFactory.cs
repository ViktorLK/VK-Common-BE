using System;
using System.Collections.Generic;
using System.Diagnostics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Factory for constructing standardized Eidos materialization response envelopes and confidence governance metadata.
/// </summary>
public static class VKMaterializationEnvelopeFactory
{
    public static VKMaterializationEnvelope<object> Create(
        object? model,
        string? rawContent,
        VKAIEidosExpressionMode effectiveMode,
        VKAIEidosExpressionMode initialMode,
        VKAIEidosResponseContract contract,
        IReadOnlyList<string> issues,
        List<VKMaterializationValidationError> accumulatedErrors,
        int repairAttempts,
        VKMaterializationToleranceMode toleranceMode,
        long startTimestamp)
    {
        return Create<object>(
            model,
            rawContent,
            effectiveMode,
            initialMode,
            contract,
            issues,
            accumulatedErrors,
            repairAttempts,
            toleranceMode,
            startTimestamp);
    }

    public static VKMaterializationEnvelope<T> Create<T>(
        T? model,
        string? rawContent,
        VKAIEidosExpressionMode effectiveMode,
        VKAIEidosExpressionMode initialMode,
        VKAIEidosResponseContract contract,
        IReadOnlyList<string> issues,
        List<VKMaterializationValidationError> accumulatedErrors,
        int repairAttempts,
        VKMaterializationToleranceMode toleranceMode,
        long startTimestamp) where T : class
    {
        VKGuard.NotNull(contract);

        var durationMs = (long)Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        var complianceScore = ComputeComplianceScore(model, accumulatedErrors, contract.Schema);

        var confidence = new VKMaterializationConfidenceMetadata
        {
            EffectiveMode = effectiveMode,
            IsDegraded = effectiveMode != initialMode,
            RepairAttempts = repairAttempts,
            ComplianceScore = complianceScore,
            ToleranceMode = toleranceMode,
            AccumulatedErrors = accumulatedErrors ?? [],
            DurationMs = durationMs
        };

        return new VKMaterializationEnvelope<T>
        {
            Model = model,
            RawContent = rawContent,
            ExpressionMode = effectiveMode,
            ContractVersion = contract.Version,
            Issues = issues ?? [],
            Confidence = confidence
        };
    }

    private static double ComputeComplianceScore(
        object? model,
        List<VKMaterializationValidationError>? errors,
        VKAIEidosSchema schema)
    {
        if (model is not null && (errors is null || errors.Count == 0))
        {
            return 1.0;
        }

        if (errors is null || errors.Count == 0)
        {
            return model is not null ? 1.0 : 0.0;
        }

        // Base field count for normalization (at least 1 to prevent division by zero)
        int fieldBase = Math.Max(1, schema.RequiredProperties.Count);
        double totalWeightedPenalty = 0.0;

        foreach (var err in errors)
        {
            totalWeightedPenalty += err.Category switch
            {
                VKMaterializationErrorCategory.Syntax => 1.0 * fieldBase,
                VKMaterializationErrorCategory.MissingRequired => 1.0,
                VKMaterializationErrorCategory.TypeMismatch => 0.75,
                VKMaterializationErrorCategory.ConstraintViolation => 0.5,
                VKMaterializationErrorCategory.ExtraneousFields => 0.2,
                _ => 0.3
            };
        }

        double normalizedRatio = totalWeightedPenalty / fieldBase;
        return Math.Max(0.0, Math.Round(1.0 - normalizedRatio, 2));
    }
}
