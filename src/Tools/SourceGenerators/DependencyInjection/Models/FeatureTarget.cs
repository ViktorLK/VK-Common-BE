using System.Collections.Immutable;

namespace VK.Tools.SourceGenerators.DependencyInjection.Models;

internal sealed record FeatureTarget(
    FeatureIdentity Identity,
    FeatureOptionsInfo Options,
    FeatureParentInfo Parent,
    int ArgsGenerationMode,
    bool RegisterByDefault,
    ImmutableArray<PropertyTarget> Properties,
    ArgsBaseInfo? ArgsBase = null);

internal sealed record FeatureIdentity(
    string Namespace,
    string FeatureName,
    string BuilderTypeFullName
);

internal sealed record FeatureOptionsInfo(
    string ClassName,
    string FullNamespace,
    string ComputedSectionName,
    bool IsToggleable,
    bool IsPartial,
    bool IsTimeoutPresent
);

internal sealed record FeatureParentInfo(
    string BlockTypeFullName,
    string? OptionsTypeFullName,
    bool Toggleable
);
