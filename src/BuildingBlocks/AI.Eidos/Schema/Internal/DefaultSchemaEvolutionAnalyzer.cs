using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Schema.Internal;

internal sealed class DefaultSchemaEvolutionAnalyzer(IVKExtractionValidator validator) : IVKSchemaEvolutionAnalyzer
{
    private readonly IVKExtractionValidator _validator = VKGuard.NotNull(validator);

    public Task<VKResult<VKSchemaEvolutionAnalysisReport>> AnalyzeEvolutionAsync(
        VKAIEidosSchema sourceSchema,
        VKAIEidosSchema targetSchema,
        IReadOnlyList<string> historicalPayloads,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(sourceSchema);
        VKGuard.NotNull(targetSchema);
        VKGuard.NotNull(historicalPayloads);

        cancellationToken.ThrowIfCancellationRequested();

        var compatResult = CheckCompatibility(sourceSchema, targetSchema);
        if (compatResult.IsFailure)
        {
            return Task.FromResult(VKResult.Failure<VKSchemaEvolutionAnalysisReport>(compatResult.Errors));
        }

        var sampleResults = new List<VKSchemaEvolutionSampleResult>(historicalPayloads.Count);
        int passed = 0;
        int failed = 0;

        for (int i = 0; i < historicalPayloads.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = historicalPayloads[i];

            if (string.IsNullOrWhiteSpace(payload))
            {
                failed++;
                sampleResults.Add(new VKSchemaEvolutionSampleResult
                {
                    SampleIndex = i,
                    IsValid = false,
                    Errors =
                    [
                        new VKExtractionValidationError
                        {
                            Category = VKExtractionErrorCategory.Syntax,
                            PropertyPath = "$",
                            Message = "Payload sample is empty or whitespace."
                        }
                    ]
                });
                continue;
            }

            var valRes = _validator.Validate(payload, targetSchema);
            var isValid = valRes.IsSuccess && valRes.Value.IsValid;

            if (isValid)
            {
                passed++;
                sampleResults.Add(new VKSchemaEvolutionSampleResult
                {
                    SampleIndex = i,
                    IsValid = true,
                    Errors = []
                });
            }
            else
            {
                failed++;
                var errors = valRes.Value?.Errors ?? [];
                sampleResults.Add(new VKSchemaEvolutionSampleResult
                {
                    SampleIndex = i,
                    IsValid = false,
                    Errors = errors
                });
            }
        }

        var report = new VKSchemaEvolutionAnalysisReport
        {
            Compatibility = compatResult.Value,
            TotalSamples = historicalPayloads.Count,
            PassedSamples = passed,
            FailedSamples = failed,
            SampleResults = sampleResults
        };

        return Task.FromResult(VKResult.Success(report));
    }

    private static VKResult<VKSchemaCompatibilityReport> CheckCompatibility(VKAIEidosSchema sourceSchema, VKAIEidosSchema targetSchema)
    {
        if (string.Equals(sourceSchema.Fingerprint, targetSchema.Fingerprint, System.StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(sourceSchema.Fingerprint))
        {
            return VKResult.Success(new VKSchemaCompatibilityReport
            {
                Level = VKSchemaCompatibilityLevel.Identical,
                Changes = []
            });
        }

        try
        {
            using var srcDoc = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(sourceSchema.RawJsonSchema) ? "{}" : sourceSchema.RawJsonSchema);
            using var tgtDoc = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(targetSchema.RawJsonSchema) ? "{}" : targetSchema.RawJsonSchema);

            var changes = new List<VKSchemaCompatibilityChange>();
            var srcRoot = srcDoc.RootElement;
            var tgtRoot = tgtDoc.RootElement;

            var srcType = GetTypeString(srcRoot);
            var tgtType = GetTypeString(tgtRoot);
            if (srcType != tgtType)
            {
                changes.Add(new VKSchemaCompatibilityChange
                {
                    PropertyPath = "$",
                    Level = VKSchemaCompatibilityLevel.Breaking,
                    Description = $"Root schema type changed from '{srcType}' to '{tgtType}'."
                });
            }

            var srcProps = GetProperties(srcRoot);
            var tgtProps = GetProperties(tgtRoot);
            var srcRequiredSet = new HashSet<string>(sourceSchema.RequiredProperties, System.StringComparer.OrdinalIgnoreCase);
            var tgtRequiredSet = new HashSet<string>(targetSchema.RequiredProperties, System.StringComparer.OrdinalIgnoreCase);

            foreach (var (propName, srcPropDef) in srcProps)
            {
                if (!tgtProps.TryGetValue(propName, out var tgtPropDef))
                {
                    changes.Add(new VKSchemaCompatibilityChange
                    {
                        PropertyPath = $"$.{propName}",
                        Level = VKSchemaCompatibilityLevel.Breaking,
                        Description = $"Property '{propName}' was removed."
                    });
                    continue;
                }

                var srcPropType = GetTypeString(srcPropDef);
                var tgtPropType = GetTypeString(tgtPropDef);
                if (srcPropType != tgtPropType)
                {
                    changes.Add(new VKSchemaCompatibilityChange
                    {
                        PropertyPath = $"$.{propName}",
                        Level = VKSchemaCompatibilityLevel.Breaking,
                        Description = $"Property '{propName}' type changed from '{srcPropType}' to '{tgtPropType}'."
                    });
                }
            }

            foreach (var (propName, _) in tgtProps)
            {
                if (!srcProps.ContainsKey(propName))
                {
                    var isRequiredInTgt = tgtRequiredSet.Contains(propName);
                    changes.Add(new VKSchemaCompatibilityChange
                    {
                        PropertyPath = $"$.{propName}",
                        Level = isRequiredInTgt ? VKSchemaCompatibilityLevel.Breaking : VKSchemaCompatibilityLevel.Compatible,
                        Description = isRequiredInTgt
                            ? $"Required property '{propName}' was added."
                            : $"Optional property '{propName}' was added."
                    });
                }
            }

            foreach (var req in tgtRequiredSet)
            {
                if (!srcRequiredSet.Contains(req) && srcProps.ContainsKey(req))
                {
                    changes.Add(new VKSchemaCompatibilityChange
                    {
                        PropertyPath = $"$.{req}",
                        Level = VKSchemaCompatibilityLevel.Breaking,
                        Description = $"Existing property '{req}' became required."
                    });
                }
            }

            var overallLevel = changes.Count == 0
                ? VKSchemaCompatibilityLevel.Identical
                : changes.Exists(c => c.Level == VKSchemaCompatibilityLevel.Breaking)
                    ? VKSchemaCompatibilityLevel.Breaking
                    : VKSchemaCompatibilityLevel.Compatible;

            return VKResult.Success(new VKSchemaCompatibilityReport
            {
                Level = overallLevel,
                Changes = changes
            });
        }
        catch (System.Exception ex)
        {
            return VKResult.Failure<VKSchemaCompatibilityReport>(
                VKError.Failure("Eidos.CompatibilityCheckFailed", $"Failed to parse schemas for compatibility analysis: {ex.Message}"));
        }
    }

    private static string GetTypeString(System.Text.Json.JsonElement element)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object && element.TryGetProperty("type", out var typeProp))
        {
            return typeProp.GetString() ?? "object";
        }
        return "object";
    }

    private static Dictionary<string, System.Text.Json.JsonElement> GetProperties(System.Text.Json.JsonElement element)
    {
        var result = new Dictionary<string, System.Text.Json.JsonElement>(System.StringComparer.OrdinalIgnoreCase);
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object && element.TryGetProperty("properties", out var props) && props.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var prop in props.EnumerateObject())
            {
                result[prop.Name] = prop.Value;
            }
        }
        return result;
    }
}
