using System.Collections.Immutable;

namespace VK.Tools.SourceGenerators.Feature.Models;

internal sealed record FeatureTarget(
    string Namespace,
    string OptionsClassName,
    string OptionsFullNamespace,
    string FeatureName,
    string ParentBlockTypeFullName,
    string BuilderTypeFullName,
    bool GenerateArgs,
    bool IsDefault,
    string? SectionNameOverride,
    bool IsToggleable,
    bool IsPartial,
    bool IsAISettings,
    bool IsGovernanceSettings,
    bool IsTimeoutPresent,
    ImmutableArray<string> ImplementedOverrides,
    ImmutableArray<PropertyTarget> Properties,
    string ComputedSectionName);
